using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using RData.Ui.Events;
using RData.Tools;

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
            if (Input.touchSupported)
            {
                for (int i = 0; i < Input.touchCount; ++i)
                {
                    var touch = Input.GetTouch(i);
                    if (touch.phase == TouchPhase.Began)
                        RegisterClick(touch.position, i, true);
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                    RegisterClick(new Vector2(Input.mousePosition.x, Input.mousePosition.y), 0);

                if (Input.GetMouseButtonDown(1))
                    RegisterClick(new Vector2(Input.mousePosition.x, Input.mousePosition.y), 1);

                if (Input.GetMouseButtonDown(2))
                    RegisterClick(new Vector2(Input.mousePosition.x, Input.mousePosition.y), 2);
            }
        }

        private void RegisterClick(Vector2 screenPoint, int button, bool isTouch = false)
        {
            if (!_rdataClient.Authorized)
                return;

            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = screenPoint;

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

            foreach (var result in results)
            {
                var selectedGameObject = result.gameObject;
                var tracker = selectedGameObject.GetComponent<RectTransformTracker>();
                if (tracker == null)
                    continue; // Continue searching for the RectTransformTracker

                Vector2 relativePosition;
                bool isHit = RectTransformHelper.ScreenPointToLocalPointInRectangleRelative(tracker.RectTransform, screenPoint, null, out relativePosition); // TODO: Provide camera for another types of canvas modes

                if (!isHit)
                    continue; // Redundant check, but who knows right?

                Debug.Log("Clicked on " + tracker.gameObject.name + ", GameObjectGuid = " + tracker.GameObjectGuid + "; tracker.Context.Id = " + tracker.Context.Id + "; position.x = " + relativePosition.x + "; position.y = " + relativePosition.y);
                var evt = new UiClickEvent(tracker.GameObjectGuid, relativePosition.x, relativePosition.y, isTouch, tracker.Context);
                RDataSingleton.Client.LogEvent(evt);

                break; // Found first RectTransformTracker, break.
            }
        }
    }
}