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

        // If sending a chunk results in an unknown error, this means something wrong with the server. 
        // To prevent spamming the server, take this timeout before re-trying to send a chunk
        private const float kTimeoutAfterError = 5.0f;

        public IJsonRpcClient JsonRpcClient { get; set; }

        public ILocalDataRepository LocalDataRepository { get; set; }

        public IAuthorizationStrategy AuthorizationStrategy { get; set; }

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

        public RDataClient()
        {
            JsonRpcClient = new JsonRpcClient();
            ChunkLifeTime = 1f; // 1 second
            ContextDataTrackRefreshTime = 0.100f; // 100 milliseconds

            LocalDataRepository = new LocalDataRepository(); // Instantiate the local data repository
            AuthorizationStrategy = new UserAuthorizationStrategy(this); // Instantiate the user authorization strategy
        }

        public IEnumerator Open(string hostName, bool waitUntilConnected = true, double waitTimeout = 3d)
        {
            yield return CoroutineManager.StartCoroutine(JsonRpcClient.Open(hostName, waitUntilConnected, waitTimeout));
        }

        public IEnumerator Close()
        {
            yield return CoroutineManager.StartCoroutine(JsonRpcClient.Close());
            EndAuthorizationContext();
            CoroutineManager.StopCoroutine(_processBulkRequestCoroutine);
            CoroutineManager.StopCoroutine(_trackContextDataCoroutine);
        }

        public void CloseImmidiately(bool stopCoroutines=true)
        {
            JsonRpcClient.CloseImmidiately();

            if (Authorized && stopCoroutines)
            {
                EndAuthorizationContext();
                CoroutineManager.StopCoroutine(_processBulkRequestCoroutine);
                CoroutineManager.StopCoroutine(_trackContextDataCoroutine);
            }
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
            _authorizationContext = null;
            LocalDataRepository.RemoveData(UserId, typeof(AuthorizationContext).Name);
        }

        private IEnumerator ProcessBulkedRequests()
        {
            while (true)
            {
                // When available, authorized, and root context is restored try to send out chunks
                if (IsAvailable && Authorized && _authorizationContext.Status != RDataContextStatus.Interrupted)
                {
                    bool hasErrors = false;
                    var localDataChunks = LocalDataRepository.LoadDataChunksJson(UserId);
                    foreach (var chunk in localDataChunks)
                    {
                        yield return CoroutineManager.StartCoroutine(JsonRpcClient.SendJson<BooleanResponse>(chunk.requestJson, chunk.requestId, (response) =>
                        {
                            if (response.Result)
                                LocalDataRepository.RemoveDataChunk(UserId, chunk.requestId); // At this point we received a positive answer from the server

                            // Most realistic scenario here is 
                            if (response.HasError)
                            {
                                if(response.Error.Data == kContextValidationError)
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
                            yield return new WaitForSeconds(kTimeoutAfterError);
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

            if(_authorizationContext != null)
                EndAuthorizationContext(); // End it
        }

        private IEnumerator TrackContextDataCoroutine()
        {
            Queue<RDataBaseContext> queue = new Queue<RDataBaseContext>();

            while (true)
            {
                // Traverse the context tree breadth-first and find new changes in the context data
                queue.Enqueue(_authorizationContext);
                while(queue.Count > 0)
                {
                    RDataBaseContext current = queue.Dequeue();
                    foreach(var kvp in current.GetUpdatedFields())
                    {
                        Debug.Log("<color=teal>Data updated in context, key = " + kvp.Key + ", value = " + kvp.Value + " </color>");
                        UpdateContextData(current, kvp.Key, kvp.Value);
                    }

                    foreach (var child in current.Children)
                        if(child.Status == RDataContextStatus.Started)
                            queue.Enqueue(child);
                }

                yield return new WaitForSeconds(ContextDataTrackRefreshTime);
            }
        }

        public virtual IEnumerator Authorize()
        {
            yield return CoroutineManager.StartCoroutine(AuthorizationStrategy.Authorize());
        }

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

            context.End();

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