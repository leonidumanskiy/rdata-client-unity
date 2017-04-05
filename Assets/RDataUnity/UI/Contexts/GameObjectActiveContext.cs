using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RData.Contexts;
using System;

namespace RData.Ui.Contexts
{
    public class GameObjectActiveContext : RDataContext<GameObjectActiveContext.GameObjectActiveContextData>
    {
        public override int ContextDataVersion
        {
            get { return 1; }
        }

        public class GameObjectActiveContextData
        {
            public string GameObjectGuid;
            public string GameObjectName;
            public string GameObjectPath;
        }

        public GameObjectActiveContext(string gameObjectGuid, string gameObjectName, string gameObjectPath) 
            : base(new GameObjectActiveContextData() { GameObjectGuid = gameObjectGuid, GameObjectName = gameObjectName, GameObjectPath = gameObjectPath })
        {
        }
    }
}