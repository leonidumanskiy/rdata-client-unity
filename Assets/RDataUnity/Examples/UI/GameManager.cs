using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RData;
using RData.Contexts;

namespace RData.Examples.UI
{
    public class GameManager : MonoBehaviour
    {
        public GameObject loadingOverlay;
        public GameObject mainWindow;

        public class MyContext : RDataContext<MyContext.MyContextData>
        {
            public class MyContextData
            {
                [TrackVar]
                public int someNumber = 0;
            }
        }

        private IEnumerator Start()
        {
            while (!RDataSingleton.Client.IsAvailable)
                yield return null;

            yield return StartCoroutine(RDataSingleton.Client.Authenticate(SystemInfo.deviceUniqueIdentifier));
            loadingOverlay.SetActive(false);
            mainWindow.SetActive(true);
            
            var testContext = new MyContext();
            RDataSingleton.Client.StartContext(testContext);
            Debug.Log("<color=yellow>starting context</color>");
            for (int i=1; i<6; i++)
            {
                yield return new WaitForSeconds(2f);
                testContext.Data.someNumber = i;
                Debug.Log("<color=yellow>someNumber = " + testContext.Data.someNumber + ", i = " + i + "</color>");
                yield return new WaitForSeconds(3f);
            }
            Debug.Log("<color=yellow>Ending context</color>");
            RDataSingleton.Client.EndContext(testContext);

        }
    }
} 
