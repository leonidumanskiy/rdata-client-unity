using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MonoBehaviourExtended : MonoBehaviour
{
    class EditorCoroutine : IEnumerator
    {
        private IEnumerator _routine;

        public EditorCoroutine _currentYieldingCoro;

        public AsyncOperation _currentYieldingAsyncOperation;

        public DateTime _startedWaiting;
        public float _waitingSeconds;

        public bool didBreak = false;

        public EditorCoroutine(IEnumerator routine)
        {
            _routine = routine;
        }

        public object Current
        {
            get { return _routine.Current; }
        }

        public bool MoveNext()
        {
            return _routine.MoveNext();
        }

        public void Reset()
        {
            _routine.Reset();
        }
    }

    private readonly List<EditorCoroutine> _activeRoutines = new List<EditorCoroutine>();

    public new object StartCoroutine(IEnumerator routine)
    {
#if UNITY_EDITOR
        if (!EditorApplication.isPlaying)
            return StartEditorCoroutine(routine);
#endif

        // In any usual occasions, use the standard coroutine
        return ((MonoBehaviour)this).StartCoroutine(routine);
    }

#if UNITY_EDITOR
    public void TestCoroutine(IEnumerator routine)
    {
        StartEditorCoroutine(routine);
        while (_activeRoutines.Count > 0)
        {
            for (int i = 0; i < _activeRoutines.Count; i++)
            {
                ProcessCoroutine(_activeRoutines[i]);
            }

            _activeRoutines.RemoveAll(r => r.didBreak);

            Thread.Sleep(1000 / 60); // Assume we have 60 fps
        }
    }

    private EditorCoroutine StartEditorCoroutine(IEnumerator routine)
    {
        var editorCoroutine = new EditorCoroutine(routine);
        _activeRoutines.Add(editorCoroutine);
        return editorCoroutine;
    }


    private void ProcessCoroutine(EditorCoroutine routine)
    {
        if (routine._currentYieldingCoro != null)
        {
            ProcessCoroutine(routine._currentYieldingCoro);
            if (routine._currentYieldingCoro.didBreak) // Inner coro is done
                routine._currentYieldingCoro = null;

            return;
        }

        if (routine._currentYieldingAsyncOperation != null)
        {
            if (routine._currentYieldingAsyncOperation.isDone) // Yilding operation is done
            {
                routine._currentYieldingAsyncOperation = null;
            }
            return;
        }

        if (routine._startedWaiting != default(DateTime))
        {
            var now = DateTime.UtcNow;
            if (now >= routine._startedWaiting + TimeSpan.FromSeconds(routine._waitingSeconds))
            {
                routine._startedWaiting = default(DateTime);
                routine._waitingSeconds = 0;
            }
            return;
        }

        if (routine.MoveNext())
        {
            var current = routine.Current;

            if (current is EditorCoroutine)
            {
                routine._currentYieldingCoro = (EditorCoroutine)current;
            }
            else if (current is AsyncOperation)
            {
                routine._currentYieldingAsyncOperation = (AsyncOperation)current;
            }
            else if (current is WaitForSeconds)
            {
                routine._startedWaiting = DateTime.UtcNow;
                routine._waitingSeconds = float.Parse(GetInstanceField(typeof(WaitForSeconds), current, "m_Seconds").ToString());
            }
            else if (current == null || current is int || current is bool)
            {
                // Do nothing. Standard yield return null or something like that
            }
            else
            {
                Debug.Log("Unknown type of yielded object: " + current.GetType());
            }
        }
        else
        {
            routine.didBreak = true;
        }
    }

    static object GetInstanceField(Type type, object instance, string fieldName)
    {
        BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
        FieldInfo field = type.GetField(fieldName, bindFlags);
        return field.GetValue(instance);
    }

#endif
}
