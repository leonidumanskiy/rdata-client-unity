using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace RData.JsonRpc
{
    public interface IJsonRpcClient
    {
        bool IsAvailable { get; }

        IEnumerator Connect(string hostName);
        IEnumerator Disconnect();
        IEnumerator Send<TRequest, TResponse>(TRequest request)
            where TRequest : JsonRpcBaseRequest
            where TResponse : JsonRpcBaseResponse;
        IEnumerator SendJson<TResponse>(string message, string id, Action<TResponse> onResponse)
            where TResponse : JsonRpcBaseResponse;
    }
}