using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RData.Tools
{
    public static class RectTransformHelper
    {
        /// <summary>
        /// Transform a screen space point into a relative percentage position in the local space of a RectTransform.
        /// </summary>
        /// <param name="rectTransform"></param>
        /// <param name="screenPoint"></param>
        /// <param name="cam"></param>
        /// <param name="relativePoint"></param>
        /// <returns></returns>
        public static bool ScreenPointToLocalPointInRectangleRelative(RectTransform rectTransform, Vector2 screenPoint, Camera cam, out Vector2 relativePoint)
        {
            Vector2 localPoint;
            var result = RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, cam, out localPoint);
            if (!result)
            {
                relativePoint = Vector2.zero;
                return false;
            }

            float x = (localPoint.x + rectTransform.pivot.x * rectTransform.rect.width) / rectTransform.rect.width;
            float y = (localPoint.y + rectTransform.pivot.y * rectTransform.rect.height) / rectTransform.rect.height;

            relativePoint = new Vector2(x, y);
            return true;
        }
    }
}