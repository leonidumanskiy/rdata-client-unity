using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace RData
{
    public class RDataLogging : MonoBehaviour
    {
        private static RDataLogging singleton;

        [SerializeField]
        private bool _showRDataDebugLog = true;
        [SerializeField]
        private bool _showRDataDebugWarning = true;
        [SerializeField]
        private bool _showRDataDebugErrors = true;

        public static bool DoLog
        {
            get
            {
                return singleton._showRDataDebugLog;
            }
            set
            {
                singleton._showRDataDebugLog = value;
            }
        }
        public static bool DoWarning
        {
            get
            {
                return singleton._showRDataDebugWarning;
            }
            set
            {
                singleton._showRDataDebugWarning = value;
            }
        }
        public static bool DoError
        {
            get
            {
                return singleton._showRDataDebugErrors;
            }
            set
            {
                singleton._showRDataDebugErrors = value;
            }
        }

        private void Awake()
        {
            singleton = this;
        }
    }
}