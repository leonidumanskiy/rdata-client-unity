using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RData.Tools
{
    public static class GameObjectHelper
    {
        /// <summary>
        /// Helper to get game object path in hierarchy
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static string GetGameObjectPath(Transform transform)
        {
            string path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = transform.name + "/" + path;
            }
            return path;
        }
    }
}