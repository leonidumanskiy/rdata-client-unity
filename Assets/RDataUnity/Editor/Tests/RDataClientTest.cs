using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using NUnit.Framework;
using RData;
using RData.Requests;
using RData.Responses;
using RData.Tests.Mock;

namespace RData.Tests
{
    [TestFixture]
    public class RDataClientTest : MonoBehaviourExtended
    {
        private MockJsonRpcClient _jsonRpcClient;
        private RDataClient _rDataClient;

        [SetUp]
        public void TestInit()
        {
            _jsonRpcClient = new MockJsonRpcClient();
            _rDataClient = new RDataClient();
            _rDataClient.JsonRpcClient = _jsonRpcClient;
            _rDataClient.Connect("hostname");
        }

        [TearDown]
        public void TestEnd()
        {
            _rDataClient.Disconnect();
            _rDataClient = null;
            _jsonRpcClient = null;
        }
        
        /*
        [Test]
        public void TestTime()
        {
            TestCoroutine(CountTime());
        }

        public IEnumerator CountTime()
        {
            for (int i = 0; i < 3; i++)
            {
                yield return new WaitForSeconds(1);
                Debug.Log(i + "; " + DateTime.Now);
            }
        }
        */

        [Test]
        public void TestMockRequest()
        {
            TestCoroutine(TestMockRequestCoro());
        }

        public IEnumerator TestMockRequestCoro()
        {
            string test = "test";
            var request = new MockRequest(test);
            var expectedResponse = new BooleanResponse(true);

            _jsonRpcClient.Expect(request, expectedResponse);
            yield return StartCoroutine(_rDataClient.Send<MockRequest, BooleanResponse>(request));
            Assert.AreEqual(request.Response.Result, expectedResponse.Result);
        }

        [Test]
        public void TestAuthentication()
        {
            TestCoroutine(TestAuthenticationCoro());
        }

        public IEnumerator TestAuthenticationCoro()
        {
            var userId = "testUser";
            var request = new Requests.User.AuthenticateRequest(userId);
            var expectedResponse = new BooleanResponse(true);

            _jsonRpcClient.Expect(request, expectedResponse);
            yield return StartCoroutine(_rDataClient.Send<Requests.User.AuthenticateRequest, BooleanResponse>(request));
            Assert.AreEqual(request.Response.Result, expectedResponse.Result);
        }
    }
}