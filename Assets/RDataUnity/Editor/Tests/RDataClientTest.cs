using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using NUnit.Framework;
using RData;
using RData.Requests;
using RData.Requests.User;
using RData.Responses;
using RData.Tests.Mock;
using System.Linq;

namespace RData.Tests
{
    [TestFixture]
    public class RDataClientTest : MonoBehaviourExtended
    {
        const string TestUserId = "testUserId";

        private MockJsonRpcClient _jsonRpcClient;
        private MockDataRepository _localDataRepository;
        private RDataClient _rDataClient;

        [SetUp]
        public void TestInit()
        {
            _jsonRpcClient = new MockJsonRpcClient();
            _localDataRepository = new MockDataRepository();
            _rDataClient = new RDataClient();
            _rDataClient.JsonRpcClient = _jsonRpcClient;
            _rDataClient.LocalDataRepository = _localDataRepository;

            _rDataClient.Open("hostname");
        }

        [TearDown]
        public void TestEnd()
        {
            _rDataClient.Close();
            _rDataClient = null;
            _jsonRpcClient = null;
        }
       
        IEnumerator Authenticate() // Fixture function for authenticating before testing
        {
            _jsonRpcClient.ExpectRequestWithMethod(new AuthenticateRequest().Method, new BooleanResponse(true));
            yield return StartCoroutine(_rDataClient.Authenticate(TestUserId));
            Assert.IsTrue(_rDataClient.Authenticated, "User authentication failed");
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

            _jsonRpcClient.ExpectRequestWithId(request.Id, expectedResponse);
            yield return StartCoroutine(_rDataClient.Send<MockRequest, BooleanResponse>(request));
            Assert.AreEqual(request.Response.Result, expectedResponse.Result, "Request results don't match");
        }

        [Test]
        public void TestAuthenticationRequest()
        {
            TestCoroutine(TestAuthenticationRequestCoro());
        }

        public IEnumerator TestAuthenticationRequestCoro()
        {
            var request = new AuthenticateRequest(TestUserId);
            var expectedResponse = new BooleanResponse(true);

            _jsonRpcClient.ExpectRequestWithId(request.Id, expectedResponse);
            yield return StartCoroutine(_rDataClient.Send<AuthenticateRequest, BooleanResponse>(request));
            Assert.AreEqual(request.Response.Result, expectedResponse.Result, "Request returned false");
        }

        [Test]
        public void TestAuthentication()
        {
            TestCoroutine(TestAuthenticationCoro());
        }

        public IEnumerator TestAuthenticationCoro()
        {
            var expectedResponse = new BooleanResponse(true);

            _jsonRpcClient.ExpectRequestWithMethod(new AuthenticateRequest().Method, expectedResponse);
            yield return StartCoroutine(_rDataClient.Authenticate(TestUserId));

            Assert.IsTrue(_rDataClient.Authenticated, "Client is not authenticated");
        }

        [Test]
        public void TestLogEvent()
        {
            TestCoroutine(TestLogEventCoro());
        }

        public IEnumerator TestLogEventCoro()
        {
            yield return StartCoroutine(Authenticate());

            _jsonRpcClient.ExpectRequestWithMethod(new Requests.System.BulkRequest().Method, new BooleanResponse(true));

            string testData = "test data";
            _rDataClient.LogEvent(new MockEventData(testData));

            Assert.AreEqual(_localDataRepository.LoadDataChunksJson(TestUserId).Count(), 0, "Local repository still has items in it");
        }
    }
}