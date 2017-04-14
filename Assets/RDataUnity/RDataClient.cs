using UnityEngine;
using System;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using RData.Authorization;
using RData.Data;
using RData.JsonRpc;
using RData.Requests;
using RData.Responses;
using RData.Exceptions;
using RData.Contexts;
using RData.Contexts.Authorization;
using RData.Events;
using RData.Requests.System;

namespace RData
{
    public class RDataClient
    {
        private const string kContextValidationError = "Context validation failed";

        /// <summary>
        /// If sending a chunk results in an unknown error, this means something wrong with the server. 
        /// To prevent spamming the server, take this timeout before re-trying to send a chunk
        /// </summary>
        private const float kTimeoutAfterError = 5.0f;
        
        public IJsonRpcClient JsonRpcClient { get; set; }

        public ILocalDataRepository LocalDataRepository { get; set; }

        public IAuthorizationStrategy AuthorizationStrategy { get; set; }

        public int GameVersion { get; set; }

        public float ChunkLifeTime { get; set; }

        public float ContextDataTrackRefreshTime { get; set; }

        public JsonRpcError<string> LastError { get; private set; }

        public bool IsAvailable
        {
            get { return JsonRpcClient.IsAvailable; }
        }

        public bool Authorized { get { return AuthorizationStrategy.Authorized; } }

        public string UserId { get { return AuthorizationStrategy.UserId; } }

        private BulkRequest _activeChunk = new BulkRequest();

        private AuthorizationContext _authorizationContext; // This is the root of our context tree
        
        private IEnumerator _processBulkRequestCoroutine;
        private IEnumerator _trackContextDataCoroutine;
        private Queue<RDataBaseContext> _contextTreeTraversalHelperQueue = new Queue<RDataBaseContext>(); // Used to not allocate a new queue when traversing the context tree

        public RDataClient()
        {
            JsonRpcClient = new JsonRpcClient();
            ChunkLifeTime = 1f; // 1 second
            ContextDataTrackRefreshTime = 0.100f; // 100 milliseconds
            GameVersion = 1;

            LocalDataRepository = new LocalDataRepository(); // Instantiate the local data repository
            AuthorizationStrategy = new UserAuthorizationStrategy(this); // Instantiate the user authorization strategy
        }

        public IEnumerator Open(string hostName, bool waitUntilConnected = true, double waitTimeout = 3d)
        {
            yield return CoroutineManager.StartCoroutine(JsonRpcClient.Open(hostName, waitUntilConnected, waitTimeout));
        }

        /// <summary>
        /// Closes the connection and yields (waits for it to be closed)
        /// </summary>
        public IEnumerator Close()
        {
            if (Authorized)
            {
                // End the root context
                if (_authorizationContext != null)
                    EndAuthorizationContext();

                // Force client to send the data to the server immidiately, ignore the ChunkLifeTime and chunk max length
                ResetActiveChunk();

                // Wait for the next ProcessBulkedRequests call to send the chunk
                yield return CoroutineManager.StartCoroutine(WaitForAllChunksToBeProcessed());

                if (_processBulkRequestCoroutine != null)
                    CoroutineManager.StopCoroutine(_processBulkRequestCoroutine);

                if (_trackContextDataCoroutine != null)
                    CoroutineManager.StopCoroutine(_trackContextDataCoroutine);

                // Unsubscribe from events
                JsonRpcClient.OnLostConnection -= OnLostConnection;
                JsonRpcClient.OnReconnected -= OnReconnected;

                // Reset Authorized on the AuthorizationStartategy
                AuthorizationStrategy.ResetAuthorization();
            }

            yield return CoroutineManager.StartCoroutine(JsonRpcClient.Close());
        }

        /// <summary>
        /// Closes the connection and doesn't yield. 
        /// </summary>
        /// <param name="stopCoroutines">If false, coroutines would not stop (used for stopping the game in the Unity editor)</param>
        public void CloseImmidiately(bool stopCoroutines=true)
        {
            if (Authorized && stopCoroutines)
            {
                if (_authorizationContext != null)
                    EndAuthorizationContext();

                if (_processBulkRequestCoroutine != null)
                    CoroutineManager.StopCoroutine(_processBulkRequestCoroutine);

                if (_trackContextDataCoroutine != null)
                    CoroutineManager.StopCoroutine(_trackContextDataCoroutine);

                // Unsubscribe from events
                JsonRpcClient.OnLostConnection -= OnLostConnection;
                JsonRpcClient.OnReconnected -= OnReconnected;
            }

            JsonRpcClient.CloseImmidiately();
        }

        public IEnumerator Send<TRequest, TResponse>(TRequest request, bool immediately = false)
            where TRequest : JsonRpcBaseRequest
            where TResponse : JsonRpcBaseResponse
        {
            if (immediately || !request.IsBulked)
            {
                yield return CoroutineManager.StartCoroutine(JsonRpcClient.Send<TRequest, TResponse>(request)); // Send request immediately
            }
            else
            {
                if (!Authorized)
                    throw new RDataException("You need to be authorized to send " + typeof(TRequest).Name);

                _activeChunk.AddRequest(request);
            }
        }

        private void OnLostConnection()
        {
            _authorizationContext.Status = RDataContextStatus.Interrupted;
        }

        private void OnReconnected()
        {
            CoroutineManager.StartCoroutine(OnReconnectedCoro());
        }

        private IEnumerator OnReconnectedCoro()
        {
            // Re-authorize
            yield return CoroutineManager.StartCoroutine(AuthorizationStrategy.RestoreAuthorization());

            // Restore interrupted contexts
            yield return CoroutineManager.StartCoroutine(RestoreInterruptedAuthorizationContext());
        }

        private void StartAuthorizationContext()
        {
            _authorizationContext = new AuthorizationContext();
            StartRootContext(_authorizationContext);
            LocalDataRepository.SaveData(UserId, typeof(AuthorizationContext).Name, _authorizationContext);
        }

        private void EndAuthorizationContext()
        {
            EndContext(_authorizationContext);
            ResetActiveChunk(); // In case if this context end fails, this will fail next authorizaiton context. Prevent it by putting them into different chunks

            LocalDataRepository.RemoveData(UserId, typeof(AuthorizationContext).Name);
        }

        /// <summary>
        /// Waits for all user data chunks to be sent to the server
        /// </summary>
        private IEnumerator WaitForAllChunksToBeProcessed()
        {
            while (LocalDataRepository.LoadDataChunksJson(UserId).ToList().Count > 0)
                yield return null;
        }

        private IEnumerator ProcessBulkedRequests()
        {
            while (true)
            {
                //Debug.Log(DateTime.UtcNow + ": Checking bulked requests");

                // When available, authorized, and root context is restored try to send out chunks
                if (IsAvailable && Authorized && _authorizationContext.Status != RDataContextStatus.Interrupted)
                {
                    bool hasErrors = false;
                    var localDataChunks = LocalDataRepository.LoadDataChunksJson(UserId);

                    if (localDataChunks != null)
                    {
                        foreach (var chunk in localDataChunks)
                        {
                            Debug.Log(DateTime.UtcNow + ": Sending the chunk " + chunk.requestId);
                            yield return CoroutineManager.StartCoroutine(JsonRpcClient.SendJson<BooleanResponse>(chunk.requestJson, chunk.requestId, (response) =>
                            {
                                Debug.Log(DateTime.UtcNow + ": Sent the chunk " + chunk.requestId);

                                if (response.Result)
                                    LocalDataRepository.RemoveDataChunk(UserId, chunk.requestId); // At this point we received a positive answer from the server

                                // Most realistic scenario here is 
                                if (response.HasError)
                                {
                                    if (response.Error.Data == kContextValidationError)
                                    {
                                        // This is a very specific case that happens when we are trying to re-send a chunk with context operations after that context was closed.
                                        // This means this chunk was already received by the server and we can safely delete it.
                                        LocalDataRepository.RemoveDataChunk(UserId, chunk.requestId);
                                        Debug.LogError("Context validation error. This chunk was already received by the server. Deletting the chunk");
                                    }
                                    else
                                    {
                                        hasErrors = true;
                                    }
                                }

                            }));

                            // If any unknown errors happened this means that most likely something horribly wrong with the server.
                            // Let's take some timeout to prevent spamming it
                            if (hasErrors)
                            {
                                Debug.Log(DateTime.UtcNow + ": Unknown error happened, waiting for " + kTimeoutAfterError + " seconds");
                                yield return new WaitForSeconds(kTimeoutAfterError);
                            }
                        }
                    }

                    // Check if the current chunk has items and expired. If so, save and refresh it
                    if (_activeChunk.Length > 0 && DateTime.UtcNow > Tools.Time.UnixTimeMillisecondsToDateTime(_activeChunk.CreatedAt) + TimeSpan.FromSeconds(ChunkLifeTime))
                    {
                        ResetActiveChunk();
                    }
                }

                yield return null;
            }
        }

        private void ResetActiveChunk()
        {
            SaveActiveChunk();
            _activeChunk = new BulkRequest();
        }

        private void SaveActiveChunk()
        {
            LocalDataRepository.SaveDataChunk(UserId, _activeChunk);
        }

        private IEnumerator RestoreInterruptedAuthorizationContext()
        {
            var request = new Requests.Contexts.RestoreContextRequest(_authorizationContext);
            yield return CoroutineManager.StartCoroutine(Send<Requests.Contexts.RestoreContextRequest, BooleanResponse>(request, true));
            _authorizationContext.Status = RDataContextStatus.Started;
        }

        private void EndInterruptedAuthorizationContext()
        {
            _authorizationContext = LocalDataRepository.LoadData<AuthorizationContext>(UserId, typeof(AuthorizationContext).Name); // Load previously saved authorization context
            
            if (_authorizationContext != null)
                EndAuthorizationContext(); // End it
        }

        private IEnumerator TrackContextDataCoroutine()
        {
            _contextTreeTraversalHelperQueue = new Queue<RDataBaseContext>();

            while (true)
            {
                CheckContextDataUpdates();
                yield return new WaitForSeconds(ContextDataTrackRefreshTime);
            }
        }

        private void CheckContextDataUpdates()
        {
            if (_contextTreeTraversalHelperQueue.Count > 0)
                throw new RDataException("CheckContextDataUpdates() was called in the middle of the context tree traversal. That is a bug and should never happen.");

            // Traverse the context tree breadth-first and find new changes in the context data
            _contextTreeTraversalHelperQueue.Enqueue(_authorizationContext);
            while (_contextTreeTraversalHelperQueue.Count > 0)
            {
                RDataBaseContext current = _contextTreeTraversalHelperQueue.Dequeue();
                foreach (var kvp in current.GetUpdatedFields())
                {
                    Debug.Log("<color=teal>Data updated in context " + current.Name + " , key=" + kvp.Key + ", value=" + kvp.Value + " </color>");
                    UpdateContextData(current, kvp.Key, kvp.Value);
                }

                foreach (var child in current.Children)
                    if (child.Status == RDataContextStatus.Started)
                        _contextTreeTraversalHelperQueue.Enqueue(child);
            }
        }

        public virtual IEnumerator Authorize()
        {
            yield return CoroutineManager.StartCoroutine(AuthorizationStrategy.Authorize());
        }

        /// <summary>
        /// This is called by the AuthorizationStrategy when it finishes the authorization
        /// </summary>
        public virtual void OnAuthorized()
        {
            // Initialize events
            JsonRpcClient.OnLostConnection += OnLostConnection;
            JsonRpcClient.OnReconnected += OnReconnected;

            // Initialize rdata client after authorization
            EndInterruptedAuthorizationContext(); // First, load previously saved root auth context and end it properly
            StartAuthorizationContext(); // Then, start new root context

            // Start bulk request processing coroutine
            _processBulkRequestCoroutine = ProcessBulkedRequests();
            CoroutineManager.StartCoroutine(_processBulkRequestCoroutine);

            // Start context data tracking
            _trackContextDataCoroutine = TrackContextDataCoroutine();
            CoroutineManager.StartCoroutine(_trackContextDataCoroutine);
        }
                
        public void LogEvent<TEventData>(RDataEvent<TEventData> evt, bool immediately = false)
            where TEventData : class, new()
        {
            if (!Authorized)
                throw new RDataException("Failed to log event " + evt.Name + ", not authorized");

            var request = new Requests.Events.LogEventRequest<TEventData>(evt);
            CoroutineManager.StartCoroutine(Send<Requests.Events.LogEventRequest<TEventData>, BooleanResponse>(request, immediately));
        }

        public void StartContext<TContextData>(RDataContext<TContextData> context, RDataBaseContext parentContext = null, bool immediately = false)
            where TContextData : class, new()
        {
            if (!Authorized)
                throw new RDataException("Failed to start context " + context.Name  + ", not authorized");

            if (parentContext == null)
                parentContext = _authorizationContext;

            parentContext.AddChild(context);

            var request = new Requests.Contexts.StartContextRequest<TContextData>(context);
            CoroutineManager.StartCoroutine(Send<Requests.Contexts.StartContextRequest<TContextData>, BooleanResponse>(request, immediately));
        }

        private void StartRootContext<TContextData>(RDataContext<TContextData> context, bool immediately = false)
            where TContextData : class, new()
        {
            if (!Authorized)
                throw new RDataException("Failed to start root context " + context.Name + ", not authorized");

            var request = new Requests.Contexts.StartContextRequest<TContextData>(context);
            CoroutineManager.StartCoroutine(Send<Requests.Contexts.StartContextRequest<TContextData>, BooleanResponse>(request, immediately));
        }

        public void EndContext(RDataBaseContext context, bool immediately = false)
        {
            if (!Authorized)
                throw new RDataException("Failed to end context " + context.Name + ", not authorized");

            CheckContextDataUpdates(); // Check the updates of the context data first

            context.End(); // Then, end the context

            var request = new Requests.Contexts.EndContextRequest(context);
            CoroutineManager.StartCoroutine(Send<Requests.Contexts.EndContextRequest, BooleanResponse>(request, immediately));
        }

        public void RestoreContext(RDataBaseContext context, bool immediately = false)
        {
            if (!Authorized)
                throw new RDataException("Failed to restore context " + context.Name + ", not authorized");

            var request = new Requests.Contexts.RestoreContextRequest(context);
            CoroutineManager.StartCoroutine(Send<Requests.Contexts.RestoreContextRequest, BooleanResponse>(request, immediately));
        }

        public void SetContextData<TContextData>(RDataContext<TContextData> context, TContextData data, bool immediately = false)
            where TContextData : class, new()
        {
            if (!Authorized)
                throw new RDataException("Failed to set data for context " + context.Name + ", not authorized");

            var request = new Requests.Contexts.SetContextDataRequest<TContextData>(context, data, Tools.Time.UnixTimeMilliseconds);
            CoroutineManager.StartCoroutine(Send<Requests.Contexts.SetContextDataRequest<TContextData>, BooleanResponse>(request, immediately));
        }

        public void UpdateContextData(RDataBaseContext context, string key, object value, bool immediately = false)
        {
            if (!Authorized)
                throw new RDataException("Failed to set data for context " + context.Name + ", not authorized");

            var request = new Requests.Contexts.UpdateContextDataVariableRequest(context, key, value, Tools.Time.UnixTimeMilliseconds);
            CoroutineManager.StartCoroutine(Send<Requests.Contexts.UpdateContextDataVariableRequest, BooleanResponse>(request, immediately));
        }
        
    }
}