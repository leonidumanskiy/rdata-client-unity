using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RData.JsonRpc
{
    public interface IJsonRpcClient
    {
        IEnumerator Connect(string hostName);
        IEnumerator Disconnect();
        IEnumerator Send<TRequest, TResponse>(TRequest request)
            where TRequest : JsonRpcBaseRequest
            where TResponse : JsonRpcBaseResponse;
    }
}