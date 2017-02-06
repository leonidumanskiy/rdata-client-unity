using UnityEngine;
using System.Collections;

/// <summary>
/// Simple class that let's you run
/// coroutines from non-MonoBehavior classes
/// </summary>
public class CoroutineManager : MonoBehaviour
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
                if(_instance == null)
                {
                    System.Type t = typeof(CoroutineManager);
                    _instance = new GameObject(t.FullName, t).GetComponent<CoroutineManager>();
                    DontDestroyOnLoad(_instance);
                }
            }
            return _instance;
        }
    }

    public static new Coroutine StartCoroutine(IEnumerator routine)
    {
        return ((MonoBehaviour)instance).StartCoroutine(routine);
    } 

}
