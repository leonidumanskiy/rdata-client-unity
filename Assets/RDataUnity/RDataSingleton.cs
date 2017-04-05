using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RData
{
    public class RDataSingleton : MonoBehaviour
    {
        public enum Stage { development, testing, production };

        public Stage m_stage = Stage.development;
        public string m_hostNameDevelopment = "ws://localhost:8888";
        public string m_hostNameTesting = "";
        public string m_hostNameProduction = "";

        public int m_gameVersion = 1;

        public bool m_waitUntilConnected = true;
        public double m_waitTimeout = 3f;

        private bool _isApplicationQuitting = false;
        private bool _isDuplicateInstance = false;

        private string HostName
        {
            get
            {
                switch (m_stage)
                {
                    default:
                    case Stage.development: return m_hostNameDevelopment;
                    case Stage.testing: return m_hostNameTesting;
                    case Stage.production: return m_hostNameProduction;
                }
            }
        }

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
            {
                _isDuplicateInstance = true;
                Destroy(gameObject);
            }
        }

        private void Awake()
        {
            EnsureSingleInstance();
            EnsureSingletonCreated();
            EnsureClientCreated();
        }

        IEnumerator Start()
        {
            // Setup global client configuration
            _client.GameVersion = m_gameVersion;

            // Open the connection
            yield return StartCoroutine(_client.Open(HostName, m_waitUntilConnected, m_waitTimeout));

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
            yield return StartCoroutine(_client.Open(HostName, m_waitUntilConnected, m_waitTimeout));
        }

        void OnDestroy()
        {
            if (_client == null || _isDuplicateInstance) // If we don't have a client instance, or we are destroying a duplicate instance, don't touch the static stuff
                return;

            _client.CloseImmidiately(!_isApplicationQuitting);
            Debug.Log(this.GetType().Name + " was destroyed");
        }
    }
}