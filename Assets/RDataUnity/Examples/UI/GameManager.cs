using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RData;

namespace RData.Examples.UI
{
    public class GameManager : MonoBehaviour
    {
        private RDataClient _rDataClient;

        IEnumerator Start()
        {   
            _rDataClient = new RDataClient();
            yield return StartCoroutine(_rDataClient.Open("ws://localhost:8888", true));

            if (_rDataClient.IsAvailable)
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
            _rDataClient.CloseImmidiately();
            print("GameManager was destroyed");
        }
    }
} 
