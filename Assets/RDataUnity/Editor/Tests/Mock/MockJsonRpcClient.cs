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

    private Dictionary<JsonRpcBaseRequest, JsonRpcBaseResponse> _expectedResponses = new Dictionary<JsonRpcBaseRequest, JsonRpcBaseResponse>();

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
        Assert.IsTrue(_expectedResponses.ContainsKey(request));
        request.SetResponse(_expectedResponses[request]);
        yield return null;
    }

    public void Expect<TRequest, TResponse>(TRequest request, TResponse response)
        where TRequest : JsonRpcBaseRequest
        where TResponse : JsonRpcBaseResponse
    {
        _expectedResponses[request] = response;
    }
}
