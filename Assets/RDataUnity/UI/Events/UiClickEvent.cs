using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RData.Events;
using RData.Ui.Contexts;

namespace RData.Ui.Events
{
    public class UiClickEvent : RDataEvent<UiClickEvent.UiClickEventData>
    {
        public override int EventDataVersion
        {
            get { return 1; }
        }

        public class UiClickEventData
        {
            public string GameObjectGuid;
            public float PositionX;
            public float PositionY;
            public bool IsTouch;
        }

        public UiClickEvent(string gameObjectGuid, float positionX, float positionY, bool isTouch, GameObjectActiveContext gameObjectContext)
            : base(new UiClickEventData() { GameObjectGuid = gameObjectGuid, PositionX = positionX, PositionY = positionY, IsTouch = isTouch }, gameObjectContext)
        {
        }
    }
}
