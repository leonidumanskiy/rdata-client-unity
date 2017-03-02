using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RData;
using RData.Contexts;

namespace RData.Examples.UI
{
    public class UiGameManager : MonoBehaviour
    {
        public GameObject loadingOverlay;
        public GameObject mainWindow;


        private IEnumerator Start()
        {
            while (!RDataSingleton.Client.IsAvailable)
                yield return null;

            yield return StartCoroutine(RDataSingleton.Client.Authenticate(SystemInfo.deviceUniqueIdentifier));
            loadingOverlay.SetActive(false);
            mainWindow.SetActive(true);
        }
    }
} 
