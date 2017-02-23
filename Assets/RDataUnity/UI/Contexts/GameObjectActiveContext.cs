using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RData.Contexts;

namespace RData.Ui.Contexts
{
    public class GameObjectActiveContext : RDataContext<GameObjectActiveContext.GameObjectActiveContextData>
    {
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