using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using WebSocketSharp;

namespace RData.JsonRpc
{
    public class JsonRpcClient : IJsonRpcClient
    {
        private WebSocket _webSocket;

        private Dictionary<string, string> _responses = new Dictionary<string, string>();
        
        public bool IsAvailable
        {
            get { return _webSocket != null && _webSocket.IsAlive; }
        }
        
        public IEnumerator Connect(string hostName)
        {
            _webSocket = new WebSocket(hostName);
            
            _webSocket.OnOpen += OnConnected;
            _webSocket.OnMessage += OnMessage;
            _webSocket.OnClose += OnDisconnected;
            _webSocket.OnError += OnError;

            _webSocket.ConnectAsync();

            while (!IsAvailable)
            {
                yield return null;
            }
        }

        public IEnumerator Disconnect()
        {
            _webSocket.CloseAsync();
            while (IsAvailable)
            {
                yield return null;
            }
        }

        public IEnumerator Send<TRequest, TResponse>(TRequest request)
            where TRequest : JsonRpcBaseRequest
            where TResponse : JsonRpcBaseResponse
        {
            string id = request.Id;
            string message = LitJson.JsonMapper.ToJson(request);
            lock (_responses)
            {
                _responses.Add(id, null);
            }
            _webSocket.SendAsync(message, b => { });

            while (true)
            {
                lock (_responses)
                {
                    if(_responses.ContainsKey(id))
                    {
                        var responseJson = _responses[id];
                        request.SetResponse(LitJson.JsonMapper.ToObject<TResponse>(responseJson));
                        _responses.Remove(id);
                        yield break;
                    }
                }
                yield return null;
            }
        }
        
        public IEnumerator SendJson<TResponse>(string message, string id, Action<TResponse> onResponse)
            where TResponse : JsonRpcBaseResponse
        {
            lock (_responses)
            {
                _responses.Add(id, null);
            }
            _webSocket.SendAsync(message, b => { });

            while (true)
            {
                lock (_responses)
                {
                    if (_responses.ContainsKey(id))
                    {
                        var responseJson = _responses[id];
                        _responses.Remove(id);
                        if (onResponse != null)
                            onResponse(LitJson.JsonMapper.ToObject<TResponse>(responseJson));

                        yield break;
                    }
                }
                yield return null;
            }
        }

        private void OnConnected(object sender, EventArgs e)
        {
            Debug.Log("Websocket connected");
        }

        private void OnDisconnected(object sender, EventArgs e)
        {
            Debug.Log("Websocket disconnected");
        }

        private void OnError(object sender, ErrorEventArgs e)
        {
            Debug.Log("WebSocket error: " + e.Message);
        }

        private void OnMessage(object sender, MessageEventArgs e)
        {
            var response = LitJson.JsonMapper.ToObject<JsonRpcBaseResponse>(e.Data);
            lock (_responses)
            {
                if (!_responses.ContainsKey(response.Id))
                    throw new JsonRpcException("Response with that id wasn't expected");

                _responses[response.Id] = e.Data;
            }
        }
    }
}