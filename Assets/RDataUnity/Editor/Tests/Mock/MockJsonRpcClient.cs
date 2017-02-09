using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RData.JsonRpc;
using RData.Tests.Mock;
using System;
using NUnit;
using NUnit.Framework;

public class MockJsonRpcClient : IJsonRpcClient
{

    public bool IsAvailable { get; private set; }

    private Dictionary<string, JsonRpcBaseResponse> _expectedResponses = new Dictionary<string, JsonRpcBaseResponse>();

    public IEnumerator Connect(string hostName)
    {
        IsAvailable = true;
        yield return null;
    }

    public IEnumerator Disconnect()
    {
        IsAvailable = false;
        yield return null;
    }

    public IEnumerator Send<TRequest, TResponse>(TRequest request)
        where TRequest : JsonRpcBaseRequest
        where TResponse : JsonRpcBaseResponse
    {
        Assert.IsTrue(_expectedResponses.ContainsKey(request.Id));
        request.SetResponse(_expectedResponses[request.Id]);
        yield return null;
    }

    public IEnumerator SendJson<TResponse>(string message, string requestId, Action<TResponse> onResponse) where TResponse : JsonRpcBaseResponse
    {
        Assert.IsTrue(_expectedResponses.ContainsKey(requestId));
        if(onResponse != null)
            onResponse((TResponse)_expectedResponses[requestId]);

        yield return null;
    }

    public void Expect<TRequest, TResponse>(TRequest request, TResponse response)
        where TRequest : JsonRpcBaseRequest
        where TResponse : JsonRpcBaseResponse
    {
        _expectedResponses[request.Id] = response;
    }
}
