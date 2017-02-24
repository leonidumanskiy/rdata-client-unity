using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace RData.JsonRpc
{
    public interface IJsonRpcClient
    {
        bool IsAvailable { get; }

        IEnumerator Open(string hostName, bool waitUntilConnected = true, double waitTimeout = 1d);
        IEnumerator Close();
        void CloseImmidiately();
        IEnumerator Send<TRequest, TResponse>(TRequest request)
            where TRequest : JsonRpcBaseRequest
            where TResponse : JsonRpcBaseResponse;
        IEnumerator SendJson<TResponse>(string message, string id, Action<TResponse> onResponse)
            where TResponse : JsonRpcBaseResponse;

        event Action OnLostConnection;
        event Action OnReconnected;
    }
}