using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace RData.Ui.Tracking
{
    /// <summary>
    /// This component is to be placed on the rect transform 
    /// to track the clicks within it
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class RectTransformTracker : TransformTracker
    {
        private RectTransform _rectTransform;

        public RectTransform RectTransform
        {
            get { return _rectTransform; }
        }

        protected override void Awake()
        {
            base.Awake();
            _rectTransform = GetComponent<RectTransform>();
        }
    }
}