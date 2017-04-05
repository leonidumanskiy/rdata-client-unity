using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RData
{
    /// <summary>
    /// Simple class that let's you run
    /// coroutines from non-MonoBehavior classes
    /// </summary>
    public class CoroutineManager : MonoBehaviourExtended
    {
        // Singleton
        private static CoroutineManager _instance = null;
        private static CoroutineManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameObject.FindObjectOfType<CoroutineManager>();

                    if (_instance == null)
                    {
                        System.Type t = typeof(CoroutineManager);
                        _instance = new GameObject(t.FullName, t).GetComponent<CoroutineManager>();

#if UNITY_EDITOR
                        if (!EditorApplication.isPaused)
                            return _instance;
#endif

                        DontDestroyOnLoad(_instance);
                    }
                }
                return _instance;
            }
        }

        protected void Awake()
        {
            if (GameObject.FindObjectsOfType<CoroutineManager>().Length > 1)
                Destroy(gameObject);

            DontDestroyOnLoad(gameObject);
        }

        public static new object StartCoroutine(IEnumerator routine)
        {
            return ((MonoBehaviourExtended)instance).StartCoroutine(routine);
        }

        public static new void StopCoroutine(IEnumerator routine)
        {
            ((MonoBehaviourExtended)instance).StopCoroutine(routine);
        }

    }
}