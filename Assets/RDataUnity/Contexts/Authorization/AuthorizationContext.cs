using System.Collections;
using System.Collections.Generic;
using System;
using RData.Contexts;
using UnityEngine;

namespace RData.Contexts.Authorization
{
    public class AuthorizationContext : RDataContext<AuthorizationContext.AuthorizationContextData>
    {
        public override int ContextDataVersion
        {
            get { return 1; }
        }

        public class AuthorizationContextData
        {
            /// <summary>
            /// Represents some values from UnityEngine.Application class
            /// </summary>
            public class ApplicationInfoData
            {
                public string unityVersion = Application.unityVersion;
                public string platform = Enum.GetName(typeof(RuntimePlatform), Application.platform);
                public string systemLanguage = Enum.GetName(typeof(SystemLanguage), Application.systemLanguage);
            }

            /// <summary>
            /// Represents some values from UnityEngine.SystemInfo class
            /// </summary>
            public class SystemInfoData
            {
                public string operatingSystem = SystemInfo.operatingSystem;
                public string deviceModel = SystemInfo.deviceModel;
                public string deviceType = Enum.GetName(typeof(DeviceType), SystemInfo.deviceType);
                public string deviceUniqueIdentifier = SystemInfo.deviceUniqueIdentifier;
            }
            
            public ApplicationInfoData applicationInfo = new ApplicationInfoData();
            public SystemInfoData systemInfo = new SystemInfoData();
        }
    }
}