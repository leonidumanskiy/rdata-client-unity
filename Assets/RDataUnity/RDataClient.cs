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
using RData.Events;
using RData.Requests.System;

namespace RData
{
    public class RDataClient
    {
        private const double ChunkLifeTime = 10d; // Seconds

        public IJsonRpcClient JsonRpcClient { get; set; }

        public ILocalDataRepository LocalDataRepository { get; set; }

        public JsonRpcError<string> LastError { get; private set; }

        public bool IsAvailable
        {
            get { return JsonRpcClient.IsAvailable; }
        }

        public bool Authenticated { get; private set; }

        public string UserId { get; private set; }
        
        private BulkRequest _activeChunk = new BulkRequest();
        
        public RDataClient()
        {
            JsonRpcClient = new JsonRpcClient();
            LocalDataRepository = new LocalDataRepository();
        }

        public IEnumerator Connect(string hostName)
        {
            yield return CoroutineManager.StartCoroutine(JsonRpcClient.Connect(hostName));
        } 

        public IEnumerator Disconnect()
        {
            yield return CoroutineManager.StartCoroutine(JsonRpcClient.Disconnect());
        }

        public IEnumerator Send<TRequest, TResponse>(TRequest request)
            where TRequest : JsonRpcBaseRequest
            where TResponse : JsonRpcBaseResponse
        {
            if (!request.IsBulked)
            {
                yield return CoroutineManager.StartCoroutine(JsonRpcClient.Send<TRequest, TResponse>(request)); // Send request immidiately
            } else 
            {
                if (!Authenticated)
                    throw new RDataException("You need to be authenticated to send " + typeof(TRequest).Name);

                _activeChunk.AddRequest(request);
            }
        }

        private IEnumerator ProcessBulkedRequests()
        {
            while (true)
            {
                if (IsAvailable) // When available, try to send out chunks
                {
                    var localDataChunks = LocalDataRepository.LoadDataChunksJson(UserId);
                    foreach(var chunk in localDataChunks)
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

        public virtual IEnumerator Authenticate(string userId)
        {
            var request = new Requests.User.AuthenticateRequest(userId);
            yield return CoroutineManager.StartCoroutine(Send<Requests.User.AuthenticateRequest, BooleanResponse>(request));
            if (request.Response.HasError)
            {
                LastError = request.Response.Error;
            } else
            {
                Authenticated = request.Response.Result;
                UserId = userId;

                CoroutineManager.StartCoroutine(ProcessBulkedRequests()); // Start bulk request processing
            }
        }

        public void LogEvent<TEventData>(TEventData eventData, RDataBaseContext context = null)
        {
            var evt = new RDataEvent<TEventData>(eventData, context);
            LogEvent(evt);
        }

        public void LogEvent<TEventData>(RDataEvent<TEventData> evt)
        {
            var request = new Requests.Events.LogEventRequest<TEventData>(evt);
            CoroutineManager.StartCoroutine(Send<Requests.Events.LogEventRequest<TEventData>, BooleanResponse>(request));
        }

        protected void StartContext<TContextData>(RDataContext<TContextData> context)
        {
            var request = new Requests.Contexts.StartContextRequest<TContextData>(context);
            CoroutineManager.StartCoroutine(Send<Requests.Contexts.StartContextRequest<TContextData>, BooleanResponse>(request));
        }

        public RDataContext<TContextData> StartContext<TContextData>(TContextData contextData, RDataBaseContext parentContext = null, bool persistent = false)
        {
            var context = new RDataContext<TContextData>(contextData, parentContext, persistent);
            if (parentContext != null)
                parentContext.AddChild(context);

            StartContext(context);
            return context;
        }

        public void EndContext(RDataBaseContext context)
        {
            context.End();

            var request = new Requests.Contexts.EndContextRequest(context);
            CoroutineManager.StartCoroutine(Send<Requests.Contexts.EndContextRequest, BooleanResponse>(request));
        }
        
        public void RestoreContext(RDataBaseContext context)
        {
            context.Restore();

            var request = new Requests.Contexts.RestoreContextRequest(context);
            CoroutineManager.StartCoroutine(Send<Requests.Contexts.RestoreContextRequest, BooleanResponse>(request));
        }
    }
}