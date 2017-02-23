using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RData
{
    public class RDataSingleton : MonoBehaviour
    {
        private static RDataClient _client;
        public static RDataClient Client
        {
            get
            {
                EnsureClientCreated();
                return _client;
            }
        }

        private static void EnsureClientCreated()
        {
            if (_client == null)
                _client = new RDataClient();
        }

        private void Awake()
        {
            EnsureClientCreated();
        }

        IEnumerator Start()
        {
            yield return StartCoroutine(_client.Open("ws://localhost:8888", true));

            if (_client.IsAvailable)
            {
                Debug.Log("Connected to the data collection server");
            }
            else
            {
                Debug.Log("Data collection server is not available. We will reconnect to it when it is available again.");
            }
        }

        void OnDestroy()
        {
            if (_client == null)
                return;

            _client.CloseImmidiately();
            print(this.GetType().Name + " was destroyed");
        }
    }
}