using UnityEngine;
using System;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using RData.Data;
using RData.JsonRpc;
using RData.Requests;
using RData.Responses;
using RData.Exceptions;
using RData.Contexts;
using RData.Contexts.Authentication;
using RData.Events;
using RData.Requests.System;

namespace RData
{
    public class RDataClient
    {
        public IJsonRpcClient JsonRpcClient { get; set; }

        public ILocalDataRepository LocalDataRepository { get; set; }

        public float ChunkLifeTime { get; set; }

        public float ContextDataTrackRefreshTime { get; set; }

        public JsonRpcError<string> LastError { get; private set; }

        public bool IsAvailable
        {
            get { return JsonRpcClient.IsAvailable; }
        }

        public bool Authenticated { get; private set; }

        public string UserId { get; private set; }

        private BulkRequest _activeChunk = new BulkRequest();

        private AuthenticationContext _authenticationContext; // This is the root of our context tree

        private IEnumerator _processBulkRequestCoroutine;
        private IEnumerator _trackContextDataCoroutine;

        public RDataClient()
        {
            JsonRpcClient = new JsonRpcClient();
            ChunkLifeTime = 10f;
            ContextDataTrackRefreshTime = 0.100f; // 100 milliseconds

            LocalDataRepository = new LocalDataRepository();
        }

        public IEnumerator Open(string hostName, bool waitUntilConnected = true, double waitTimeout = 3d)
        {
            yield return CoroutineManager.StartCoroutine(JsonRpcClient.Open(hostName, waitUntilConnected, waitTimeout));
        }

        public IEnumerator Close()
        {
            yield return CoroutineManager.StartCoroutine(JsonRpcClient.Close());
            EndAuthenticationContext();
            CoroutineManager.StopCoroutine(_processBulkRequestCoroutine);
            CoroutineManager.StopCoroutine(_trackContextDataCoroutine);
        }

        public void CloseImmidiately()
        {
            CoroutineManager.StartCoroutine(JsonRpcClient.Close());

            if (Authenticated)
            {
                EndAuthenticationContext();
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
                if (!Authenticated)
                    throw new RDataException("You need to be authenticated to send " + typeof(TRequest).Name);

                _activeChunk.AddRequest(request);
            }
        }

        private void OnLostConnection()
        {
            _authenticationContext.Status = RDataContextStatus.Interrupted;
        }

        private void OnReconnected()
        {
            CoroutineManager.StartCoroutine(OnReconnectedCoro());
        }

        private IEnumerator OnReconnectedCoro()
        {
            // Re-authenticate
            yield return CoroutineManager.StartCoroutine(SendAuthenticationRequest(UserId));

            // Restore interrupted contexts
            yield return CoroutineManager.StartCoroutine(RestoreInterruptedAuthenticationContext());
        }

        private void StartAuthenticationContext()
        {
            _authenticationContext = new AuthenticationContext();
            StartRootContext(_authenticationContext);
            LocalDataRepository.SaveData(UserId, typeof(AuthenticationContext).Name, _authenticationContext);
        }

        private void EndAuthenticationContext()
        {
            EndContext(_authenticationContext);
            _authenticationContext = null;
            LocalDataRepository.RemoveData(UserId, typeof(AuthenticationContext).Name);
        }

        private IEnumerator ProcessBulkedRequests()
        {
            while (true)
            {
                // When available, authenticated, and root context is restored try to send out chunks
                if (IsAvailable && Authenticated && _authenticationContext.Status != RDataContextStatus.Interrupted)
                {
                    var localDataChunks = LocalDataRepository.LoadDataChunksJson(UserId);
                    foreach (var chunk in localDataChunks)
                    {
                        yield return CoroutineManager.StartCoroutine(JsonRpcClient.SendJson<BooleanResponse>(chunk.requestJson, chunk.requestId, (response) =>
                        {
                            if (response.Result)
                                LocalDataRepository.RemoveDataChunk(UserId, chunk.requestId); // At this point we received a positive answer from the server
                            
                            // If response.Result is false, something went wrong, either on the server, or the data is corrupt
                            // What should we do? Remove chunk to prevent spamming server?
                        }));
                    }

                    // Check if the current chunk has items and expired. If so, save and refresh it
                    if (_activeChunk.Length > 0 && DateTime.UtcNow > Tools.Time.UnixTimeToDateTime(_activeChunk.CreatedAt) + TimeSpan.FromSeconds(ChunkLifeTime))
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

        private IEnumerator RestoreInterruptedAuthenticationContext()
        {
            var request = new Requests.Contexts.RestoreContextRequest(_authenticationContext);
            yield return CoroutineManager.StartCoroutine(Send<Requests.Contexts.RestoreContextRequest, BooleanResponse>(request, true));
            _authenticationContext.Status = RDataContextStatus.Started;
        }

        private void EndInterruptedAuthenticationContext()
        {
            _authenticationContext = LocalDataRepository.LoadData<AuthenticationContext>(UserId, typeof(AuthenticationContext).Name); // Load previously saved authentication context

            if(_authenticationContext != null)
                EndAuthenticationContext(); // End it
        }

        private IEnumerator TrackContextDataCoroutine()
        {
            Queue<RDataBaseContext> queue = new Queue<RDataBaseContext>();

            while (true)
            {
                // Traverse the context tree breadth-first and find new changes in the context data
                queue.Enqueue(_authenticationContext);
                while(queue.Count > 0)
                {
                    RDataBaseContext current = queue.Dequeue();
                    foreach(var kvp in current.GetUpdatedFields())
                    {
                        Debug.Log("<color=red>Data updated in context, key = " + kvp.Key + ", value = " + kvp.Value + " </color>");
                        UpdateContextData(current, kvp.Key, kvp.Value);
                    }

                    foreach (var child in current.Children)
                        if(child.Status == RDataContextStatus.Started)
                            queue.Enqueue(child);
                }

                yield return new WaitForSeconds(ContextDataTrackRefreshTime);
            }
        }

        public virtual IEnumerator Authenticate(string userId)
        {
            if (Authenticated)
                throw new RDataException("Already authenticated");

            yield return CoroutineManager.StartCoroutine(SendAuthenticationRequest(userId));

            if (Authenticated)
            {
                // Initialize events
                JsonRpcClient.OnLostConnection += OnLostConnection;
                JsonRpcClient.OnReconnected += OnReconnected;

                // Initialize rdata client after authentication
                EndInterruptedAuthenticationContext(); // First, load previously saved root auth context and end it properly
                StartAuthenticationContext(); // Then, start new root context

                // Start bulk request processing coroutine
                _processBulkRequestCoroutine = ProcessBulkedRequests();
                CoroutineManager.StartCoroutine(_processBulkRequestCoroutine);

                // Start context data tracking
                _trackContextDataCoroutine = TrackContextDataCoroutine();
                CoroutineManager.StartCoroutine(_trackContextDataCoroutine);
            }
        }

        private IEnumerator SendAuthenticationRequest(string userId)
        {
            var request = new Requests.User.AuthenticateRequest(userId);
            yield return CoroutineManager.StartCoroutine(Send<Requests.User.AuthenticateRequest, BooleanResponse>(request));
            if (request.Response.HasError)
            {
                LastError = request.Response.Error;
            }
            else
            {
                Authenticated = request.Response.Result;
                UserId = userId;
            }
        }
        
        public void LogEvent<TEventData>(RDataEvent<TEventData> evt, bool immediately = false)
        {
            if (!Authenticated)
                throw new RDataException("Failed to log event " + evt.Name + ", not authenticated");

            var request = new Requests.Events.LogEventRequest<TEventData>(evt);
            CoroutineManager.StartCoroutine(Send<Requests.Events.LogEventRequest<TEventData>, BooleanResponse>(request, immediately));
        }

        public void StartContext<TContextData>(RDataContext<TContextData> context, RDataBaseContext parentContext = null, bool immediately = false)
            where TContextData : class, new()
        {
            if (!Authenticated)
                throw new RDataException("Failed to start context " + context.Name  + ", not authenticated");

            if (parentContext == null)
                parentContext = _authenticationContext;

            parentContext.AddChild(context);

            var request = new Requests.Contexts.StartContextRequest<TContextData>(context);
            CoroutineManager.StartCoroutine(Send<Requests.Contexts.StartContextRequest<TContextData>, BooleanResponse>(request, immediately));
        }

        private void StartRootContext<TContextData>(RDataContext<TContextData> context, bool immediately = false)
            where TContextData : class, new()
        {
            if (!Authenticated)
                throw new RDataException("Failed to start root context " + context.Name + ", not authenticated");

            var request = new Requests.Contexts.StartContextRequest<TContextData>(context);
            CoroutineManager.StartCoroutine(Send<Requests.Contexts.StartContextRequest<TContextData>, BooleanResponse>(request, immediately));
        }

        public void EndContext(RDataBaseContext context, bool immediately = false)
        {
            if (!Authenticated)
                throw new RDataException("Failed to end context " + context.Name + ", not authenticated");

            context.End();

            var request = new Requests.Contexts.EndContextRequest(context);
            CoroutineManager.StartCoroutine(Send<Requests.Contexts.EndContextRequest, BooleanResponse>(request, immediately));
        }

        public void RestoreContext(RDataBaseContext context, bool immediately = false)
        {
            if (!Authenticated)
                throw new RDataException("Failed to restore context " + context.Name + ", not authenticated");

            var request = new Requests.Contexts.RestoreContextRequest(context);
            CoroutineManager.StartCoroutine(Send<Requests.Contexts.RestoreContextRequest, BooleanResponse>(request, immediately));
        }

        public void SetContextData<TContextData>(RDataContext<TContextData> context, TContextData data, bool immediately = false)
            where TContextData : class, new()
        {
            if (!Authenticated)
                throw new RDataException("Failed to set data for context " + context.Name + ", not authenticated");

            var request = new Requests.Contexts.SetContextDataRequest<TContextData>(context, data, Tools.Time.UnixTime);
            CoroutineManager.StartCoroutine(Send<Requests.Contexts.SetContextDataRequest<TContextData>, BooleanResponse>(request, immediately));
        }

        public void UpdateContextData(RDataBaseContext context, string key, object value, bool immediately = false)
        {
            if (!Authenticated)
                throw new RDataException("Failed to set data for context " + context.Name + ", not authenticated");

            var request = new Requests.Contexts.UpdateContextDataVariableRequest(context, key, value, Tools.Time.UnixTime);
            CoroutineManager.StartCoroutine(Send<Requests.Contexts.UpdateContextDataVariableRequest, BooleanResponse>(request, immediately));
        }
    }
}