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

        public double ChunkLifeTime { get; set; }

        public JsonRpcError<string> LastError { get; private set; }

        public bool IsAvailable
        {
            get { return JsonRpcClient.IsAvailable; }
        }

        public bool Authenticated { get; private set; }

        public string UserId { get; private set; }

        private BulkRequest _activeChunk = new BulkRequest();

        private AuthenticationContext _authenticationContext; // This is the root of our context tree

        public RDataClient()
        {
            JsonRpcClient = new JsonRpcClient();
            JsonRpcClient.OnReconnected += OnReconnected;
            ChunkLifeTime = 10d;

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
        }

        public void CloseImmidiately()
        {
            JsonRpcClient.CloseImmidiately();
        }

        public IEnumerator Send<TRequest, TResponse>(TRequest request)
            where TRequest : JsonRpcBaseRequest
            where TResponse : JsonRpcBaseResponse
        {
            if (!request.IsBulked)
            {
                yield return CoroutineManager.StartCoroutine(JsonRpcClient.Send<TRequest, TResponse>(request)); // Send request immidiately
            }
            else
            {
                if (!Authenticated)
                    throw new RDataException("You need to be authenticated to send " + typeof(TRequest).Name);

                _activeChunk.AddRequest(request);
            }
        }

        private void OnReconnected()
        {
            // Restore interrupted contexts
            RestoreInterruptedContexts();
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
                if (IsAvailable) // When available, try to send out chunks
                {
                    var localDataChunks = LocalDataRepository.LoadDataChunksJson(UserId);
                    foreach (var chunk in localDataChunks)
                    {
                        yield return CoroutineManager.StartCoroutine(JsonRpcClient.SendJson<BooleanResponse>(chunk.requestJson, chunk.requestId, (response) =>
                        {
                            if (response.Result)
                                LocalDataRepository.RemoveDataChunk(UserId, chunk.requestId); // At this point we received a positive answer from the server
                        }));
                    }

                    // Check if the current chunk has expired, if so, save and refresh it
                    if (DateTime.UtcNow > Tools.Time.UnixTimeToDateTime(_activeChunk.CreatedAt) + TimeSpan.FromSeconds(ChunkLifeTime))
                    {
                        SaveActiveChunk();
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

        private void RestoreInterruptedContexts()
        {
            RestoreContext(_authenticationContext);
        }

        private void EndInterruptedAuthenticationContext()
        {
            _authenticationContext = LocalDataRepository.LoadData<AuthenticationContext>(UserId, typeof(AuthenticationContext).Name); // Load previously saved authentication context

            if(_authenticationContext != null)
                EndAuthenticationContext(); // End it
        }

        public virtual IEnumerator Authenticate(string userId)
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
                
                EndInterruptedAuthenticationContext(); // First, load root auth context and end it properly
                StartAuthenticationContext(); // Now, start new root context

                CoroutineManager.StartCoroutine(ProcessBulkedRequests()); // Start bulk request processing
            }
        }
        
        public void LogEvent<TEventData>(RDataEvent<TEventData> evt)
        {
            var request = new Requests.Events.LogEventRequest<TEventData>(evt);
            CoroutineManager.StartCoroutine(Send<Requests.Events.LogEventRequest<TEventData>, BooleanResponse>(request));
        }

        public void StartContext<TContextData>(RDataContext<TContextData> context, RDataBaseContext parentContext = null)
            where TContextData : class, new()
        {
            if (parentContext == null)
                parentContext = _authenticationContext;

            parentContext.AddChild(context);

            var request = new Requests.Contexts.StartContextRequest<TContextData>(context);
            CoroutineManager.StartCoroutine(Send<Requests.Contexts.StartContextRequest<TContextData>, BooleanResponse>(request));
        }

        private void StartRootContext<TContextData>(RDataContext<TContextData> context)
            where TContextData : class, new()
        {
            var request = new Requests.Contexts.StartContextRequest<TContextData>(context);
            CoroutineManager.StartCoroutine(Send<Requests.Contexts.StartContextRequest<TContextData>, BooleanResponse>(request));
        }

        public void EndContext(RDataBaseContext context)
        {
            context.End();

            var request = new Requests.Contexts.EndContextRequest(context);
            CoroutineManager.StartCoroutine(Send<Requests.Contexts.EndContextRequest, BooleanResponse>(request));
        }

        public void RestoreContext(RDataBaseContext context)
        {
            var request = new Requests.Contexts.RestoreContextRequest(context);
            CoroutineManager.StartCoroutine(Send<Requests.Contexts.RestoreContextRequest, BooleanResponse>(request));
        }
    }
}