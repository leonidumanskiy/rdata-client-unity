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

    private Dictionary<string, JsonRpcBaseResponse> _expectedRequestIds = new Dictionary<string, JsonRpcBaseResponse>();
    private Dictionary<string, JsonRpcBaseResponse> _expectedRequestMethods = new Dictionary<string, JsonRpcBaseResponse>();

    public event Action OnLostConnection;
    public event Action OnReconnected;

    public IEnumerator Open(string hostName, bool waitUntilConnected = true, double waitTimeout = 1d)
    {
        IsAvailable = true;
        yield return null;
    }

    public IEnumerator Close()
    {
        CloseImmidiately();
        yield return null;
    }

    public void CloseImmidiately()
    {
        IsAvailable = false;
    }

    public IEnumerator Send<TRequest, TResponse>(TRequest request)
        where TRequest : JsonRpcBaseRequest
        where TResponse : JsonRpcBaseResponse
    {
        if(_expectedRequestIds.ContainsKey(request.Id))
        {
            request.SetResponse(_expectedRequestIds[request.Id]);
            yield break;
        }
        
        else if (_expectedRequestMethods.ContainsKey(request.Method))
        {
            request.SetResponse(_expectedRequestMethods[request.Method]);
            yield break;
        }                
        else
        {
            throw new Exception("Unexpected request");
        }
    }

    public IEnumerator SendJson<TResponse>(string message, string requestId, Action<TResponse> onResponse) where TResponse : JsonRpcBaseResponse
    {
        Assert.IsTrue(_expectedRequestIds.ContainsKey(requestId));
        if(onResponse != null)
            onResponse((TResponse)_expectedRequestIds[requestId]);

        yield return null;
    }

    public void ExpectRequestWithId<TResponse>(string requestId, TResponse response)
        where TResponse : JsonRpcBaseResponse
    {
        _expectedRequestIds[requestId] = response;
    }

    public void ExpectRequestWithMethod<TResponse>(string command, TResponse response)
        where TResponse : JsonRpcBaseResponse
    {
        _expectedRequestMethods[command] = response;
    }

}
