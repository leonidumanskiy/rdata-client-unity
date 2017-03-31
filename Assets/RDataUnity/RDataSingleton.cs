using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RData
{
    public class RDataSingleton : MonoBehaviour
    {
        public string m_hostName = "ws://localhost:8888";
        public bool m_waitUntilConnected = true;
        public double m_waitTimeout = 3f;

        private bool _isApplicationQuitting = false;

        private static RDataClient _client;
        public static RDataClient Client
        {
            get
            {
                EnsureClientCreated();
                return _client;
            }
        }

        private static RDataSingleton _instance;
        public static RDataSingleton instance
        {
            get { return _instance; }
        }

        void OnApplicationQuit()
        {
            _isApplicationQuitting = true;
        }

        private void EnsureSingletonCreated()
        {
            if (_instance == null)
                _instance = this;
        }

        private static void EnsureClientCreated()
        {
            if (_client == null)
                _client = new RDataClient();
        }

        private void EnsureSingleInstance()
        {
            if (FindObjectsOfType(typeof(RDataSingleton)).Length > 1)
                Destroy(gameObject);
        }

        private void Awake()
        {
            EnsureSingleInstance();
            EnsureSingletonCreated();
            EnsureClientCreated();
        }

        IEnumerator Start()
        {
            yield return StartCoroutine(_client.Open(m_hostName, m_waitUntilConnected, m_waitTimeout));

            if (_client.IsAvailable)
            {
                Debug.Log("Connected to the data collection server");
            }
            else
            {
                Debug.Log("Data collection server is not available. We will reconnect to it when it is available again.");
            }
        }

        public IEnumerator Restart()
        {
            yield return _client.Close();
            yield return StartCoroutine(_client.Open(m_hostName, m_waitUntilConnected, m_waitTimeout));
        }

        void OnDestroy()
        {
            if (_client == null)
                return;

            _client.CloseImmidiately(!_isApplicationQuitting);
            Debug.Log(this.GetType().Name + " was destroyed");
        }
    }
}