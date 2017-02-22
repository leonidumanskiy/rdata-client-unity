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

    public class ExpectedResponse
    {
        public JsonRpcBaseResponse response;
        public bool autoRemove = true;
    }

    private Dictionary<string, ExpectedResponse> _expectedRequestIds = new Dictionary<string, ExpectedResponse>();
    private Dictionary<string, ExpectedResponse> _expectedRequestMethods = new Dictionary<string, ExpectedResponse>();

    public event Action OnReconnected;

    public int NumExpectedRequests
    {
        get { return _expectedRequestIds.Count + _expectedRequestMethods.Count; }
    }

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
        if (_expectedRequestIds.ContainsKey(request.Id))
        {
            var expectation = _expectedRequestIds[request.Id];
            request.SetResponse(expectation.response);
            if (expectation.autoRemove)
                _expectedRequestIds.Remove(request.Id);

            yield break;
        }

        else if (_expectedRequestMethods.ContainsKey(request.Method))
        {
            var expectation = _expectedRequestMethods[request.Method];
            request.SetResponse(expectation.response);
            if (expectation.autoRemove)
                _expectedRequestMethods.Remove(request.Method);
            yield break;
        }
        else
        {
            throw new Exception("Unexpected request");
        }
    }

    public IEnumerator SendJson<TResponse>(string message, string requestId, Action<TResponse> onResponse) where TResponse : JsonRpcBaseResponse
    {
        var request = LitJson.JsonMapper.ToObject<JsonRpcBaseRequest>(message);
        if (_expectedRequestIds.ContainsKey(requestId))
        {
            var expectation = _expectedRequestIds[requestId];
            if (expectation.autoRemove)
                _expectedRequestIds.Remove(requestId);

            if (onResponse != null)
                onResponse((TResponse)expectation.response);

        }
        else if (_expectedRequestMethods.ContainsKey(request.Method))
        {
            var expectation = _expectedRequestMethods[request.Method];
            if (expectation.autoRemove)
                _expectedRequestMethods.Remove(request.Method);

            if (onResponse != null)
                onResponse((TResponse)expectation.response);
        }
        else
        {
            throw new Exception("Unexpected json request");
        }

        yield return null;
    }

    public void ExpectRequestWithId<TResponse>(string requestId, TResponse response)
        where TResponse : JsonRpcBaseResponse
    {
        _expectedRequestIds[requestId] = new ExpectedResponse() { response = response };
    }

    public void ExpectRequestWithMethod<TResponse>(string command, TResponse response)
        where TResponse : JsonRpcBaseResponse
    {
        _expectedRequestMethods[command] = new ExpectedResponse() { response = response };
    }

    public void TemporaryDisconnect()
    {
        OnReconnected();
    }
}
