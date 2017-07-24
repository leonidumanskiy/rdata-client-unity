using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RData.JsonRpc;
using RData.Exceptions;

namespace RData.Authorization
{
    public interface IAuthorizationStrategy
    {
        bool Authorized { get; }
        string UserId { get; set; }
        RDataException LastError { get; }

        IEnumerator Authorize(); // Called by the user when user calls Client.Authorize()     
        IEnumerator RestoreAuthorization();  // Called by the RDataClient when it reconnects and needs to re-authorize
        void ResetAuthorization();
    }
}
