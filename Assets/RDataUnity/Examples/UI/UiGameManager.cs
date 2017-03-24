using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RData;
using RData.Contexts;
using RData.Authorization;

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
            
            RDataSingleton.Client.AuthorizationStrategy.UserId = SystemInfo.deviceUniqueIdentifier;
            yield return StartCoroutine(RDataSingleton.Client.Authorize());
            
            loadingOverlay.SetActive(false);
            mainWindow.SetActive(true);
        }
    }
} 
