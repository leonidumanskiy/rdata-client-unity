using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RData.Ui.Tracking
{
    /// <summary>
    /// This class is used to track ui clicks
    /// in a form of events
    /// </summary>
    public class EventSystemTracker : MonoBehaviour
    {
        private RDataClient _rdataClient;

        private void Start()
        {
            Debug.Log("Ui tracker start");
            _rdataClient = RDataSingleton.Client;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
                RegisterClick(0, new Vector2(Input.mousePosition.x, Input.mousePosition.y));

            if (Input.GetMouseButtonDown(1))
                RegisterClick(1, new Vector2(Input.mousePosition.x, Input.mousePosition.y));

            if (Input.GetMouseButtonDown(2))
                RegisterClick(2, new Vector2(Input.mousePosition.x, Input.mousePosition.y));


            for (int i = 0; i < Input.touchCount; ++i)
            {
                var touch = Input.GetTouch(i);
                if (touch.phase == TouchPhase.Began)
                    RegisterClick(i, touch.position, true);
            }
        }

        private void RegisterClick(int button, Vector2 position, bool isTouch = false)
        {
            //if (!_rdataClient.Authenticated)
            //    return;

            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = position;

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

            foreach (var result in results)
            {
                var selectedGameObject = result.gameObject;
                var tracker = selectedGameObject.GetComponent<GameObjectTracker>();
                if (tracker == null)
                    continue;

                Debug.Log("Clicked on " + tracker.gameObject.name + " - " + tracker.GameObjectGuid);
            }
        }
    }
}