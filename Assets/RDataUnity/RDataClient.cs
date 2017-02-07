using UnityEngine;
using System;
using System.Collections;
using RData.JsonRpc;
using RData.Requests;
using RData.Responses;
using RData.Contexts;
using RData.Events;

namespace RData
{
    public class RDataClient
    {
        public IJsonRpcClient JsonRpcClient { get; set; }

        public RDataClient()
        {
            JsonRpcClient = new JsonRpcClient();
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
            yield return CoroutineManager.StartCoroutine(JsonRpcClient.Send<TRequest, TResponse>(request));
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