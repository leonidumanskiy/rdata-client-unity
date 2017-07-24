using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RData.Exceptions;
using RData.Responses;
using RData.JsonRpc;

namespace RData.Authorization
{
    public class UserAuthorizationStrategy : IAuthorizationStrategy
    {
        private RDataClient _client;

        public bool Authorized { get; private set; }

        public string UserId { get; set; }

        public RDataException LastError { get; private set; }

        public UserAuthorizationStrategy(RDataClient client)
        {
            _client = client;
        }

        public IEnumerator Authorize()
        {
            if (_client.Authorized)
            {
                LastError = new RDataException("Already authorized");
                yield break;
            }

            if (string.IsNullOrEmpty(UserId))
            {
                LastError = new RDataException("You must set the user id on the authorization strategy to authorize");
                yield break;
            }

            yield return CoroutineManager.StartCoroutine(SendAuthorizationRequest(UserId, _client.GameVersion));

            if (_client.Authorized)
                _client.OnAuthorized();
        }

        public IEnumerator RestoreAuthorization()
        {
            // For re-authorization, simply send the user id
            yield return CoroutineManager.StartCoroutine(SendAuthorizationRequest(UserId, _client.GameVersion));
        }

        public void ResetAuthorization()
        {
            Authorized = false;
        }

        private IEnumerator SendAuthorizationRequest(string userId, int gameVersion)
        {
            var request = new Requests.User.AuthorizationRequest(userId, gameVersion);
            yield return CoroutineManager.StartCoroutine(_client.Send<Requests.User.AuthorizationRequest, BooleanResponse>(request));
            if (request.Response.HasError)
            {
                LastError = new RDataException(request.Response.Error);
            }
            else
            {
                Authorized = request.Response.Result;
                UserId = userId;
            }
        }

    }
}