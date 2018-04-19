using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RData;
using RData.Authorization;
using RData.Contexts;
using System;

namespace RData.Examples.ContextDataTracking
{
    public class ContextDataTrackingGameManager : MonoBehaviour
    {
        public class MyContext : RDataContext<MyContext.MyContextData>
        {
            public override int ContextDataVersion
            {
                get { return 1; }
            }

            [TrackClass]
            public class Test
            {
                [TrackVar]
                public int someNumber = 0;
            }

            public class MyContextData
            {
                [TrackVar]
                public Test test = new Test();
            }
        }

        private IEnumerator Start()
        {
            while (!RDataSingleton.Client.IsAvailable)
                yield return null;


            RDataSingleton.Client.AuthorizationStrategy.UserId = SystemInfo.deviceUniqueIdentifier;
            yield return StartCoroutine(RDataSingleton.Client.Authorize());

            var testContext = new MyContext();
            RDataSingleton.Client.StartContext(testContext);
            if (RData.RDataLogging.DoLog)
                Debug.Log("<color=yellow>Starting MyContext</color>");
            for (int i = 1; i < 6; i++)
            {
                yield return new WaitForSeconds(2f);
                testContext.Data.test.someNumber = i;
                if (RData.RDataLogging.DoLog)
                    Debug.Log("<color=yellow>Updating test.someNumber to " + testContext.Data.test.someNumber + ", i = " + i + "</color>");
                yield return new WaitForSeconds(3f);
            }
            if (RData.RDataLogging.DoLog)
                Debug.Log("<color=yellow>Ending MyContext</color>");
            RDataSingleton.Client.EndContext(testContext);

        }
    }
}
