using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Diagnostics;
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
    public class RDataClientTest
    {
        const string TestUserId = "testUserId";
        const double TestTimeout = 5.0d; // Seconds

        private MockJsonRpcClient _jsonRpcClient;
        private MockDataRepository _localDataRepository;
        private RDataClient _rDataClient;
        private CoroutineManager _coroutineManager;
        private Stopwatch _testStopWatch;

        [SetUp]
        public void TestInit()
        {
            _testStopWatch = new Stopwatch();
            _coroutineManager = new GameObject("Test_CoroutineManager", typeof(CoroutineManager)).GetComponent<CoroutineManager>();

            _jsonRpcClient = new MockJsonRpcClient();
            _localDataRepository = new MockDataRepository();
            _rDataClient = new RDataClient();
            _rDataClient.JsonRpcClient = _jsonRpcClient;
            _rDataClient.LocalDataRepository = _localDataRepository;
            _rDataClient.ChunkLifeTime = 0.010f;

            _coroutineManager.TestCoroutine(_rDataClient.Open("hostname"));
            _testStopWatch.Start();
        }

        [TearDown]
        public void TestEnd()
        {
            GameObject.DestroyImmediate(_coroutineManager);
            _rDataClient.Close();
            _rDataClient = null;
            _jsonRpcClient = null;
            _testStopWatch.Stop();
            _testStopWatch = null;
        }

        IEnumerator Authenticate() // Fixture function for authenticating before testing
        {
            _jsonRpcClient.ExpectRequestWithMethod(new AuthenticateRequest().Method, new BooleanResponse(true));
            _jsonRpcClient.ExpectRequestWithMethod(new Requests.System.BulkRequest().Method, new BooleanResponse(true)); // Authentication context
            yield return CoroutineManager.StartCoroutine(_rDataClient.Authenticate(TestUserId));
            Assert.IsTrue(_rDataClient.Authenticated, "User authentication failed");
        }

        /*
        [Test]
        public void TestTime()
        {
            _coroutineManager.TestCoroutine(CountTime());
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
            _coroutineManager.TestCoroutine(TestMockRequestCoro());
        }

        public IEnumerator TestMockRequestCoro()
        {
            string test = "test";
            var request = new MockRequest(test);
            var expectedResponse = new BooleanResponse(true);

            _jsonRpcClient.ExpectRequestWithId(request.Id, expectedResponse);
            _jsonRpcClient.ExpectRequestWithMethod(new Requests.System.BulkRequest().Method, new BooleanResponse(true)); // Authentication context

            yield return CoroutineManager.StartCoroutine(_rDataClient.Send<MockRequest, BooleanResponse>(request));
            Assert.AreEqual(request.Response.Result, expectedResponse.Result, "Request results don't match");
        }

        [Test]
        public void TestAuthenticationRequest()
        {
            _coroutineManager.TestCoroutine(TestAuthenticationRequestCoro());
        }

        public IEnumerator TestAuthenticationRequestCoro()
        {
            var request = new AuthenticateRequest(TestUserId);
            var expectedResponse = new BooleanResponse(true);

            _jsonRpcClient.ExpectRequestWithId(request.Id, expectedResponse);
            _jsonRpcClient.ExpectRequestWithMethod(new Requests.System.BulkRequest().Method, new BooleanResponse(true)); // Authentication context

            yield return CoroutineManager.StartCoroutine(_rDataClient.Send<AuthenticateRequest, BooleanResponse>(request));
            Assert.AreEqual(request.Response.Result, expectedResponse.Result, "Request returned false");
        }
        
        [Test]
        public void TestLogEvent()
        {
            _coroutineManager.TestCoroutine(TestLogEventCoro());
        }

        public IEnumerator TestLogEventCoro()
        {
            yield return CoroutineManager.StartCoroutine(Authenticate());

            _jsonRpcClient.ExpectRequestWithMethod(new Requests.System.BulkRequest().Method, new BooleanResponse(true));

            string testData = "test data";
            _rDataClient.LogEvent(new MockEvent(testData));

            while (_jsonRpcClient.NumExpectedRequests > 0 && _testStopWatch.Elapsed < TimeSpan.FromSeconds(TestTimeout))
                yield return null;

            Assert.AreEqual(_jsonRpcClient.NumExpectedRequests, 0, "Expected request was never sent by mock json rpc client");
            Assert.AreEqual(_localDataRepository.LoadDataChunksJson(TestUserId).Count(), 0, "Local repository still has items in it");
        }


        [Test]
        public void TestStartContext()
        {
            _coroutineManager.TestCoroutine(TestStartContextCoro());
        }

        public IEnumerator TestStartContextCoro()
        {
            yield return CoroutineManager.StartCoroutine(Authenticate());

            _jsonRpcClient.ExpectRequestWithMethod(new Requests.System.BulkRequest().Method, new BooleanResponse(true));

            string testData = "test data";
            MockContext context = new MockContext(testData);
            _rDataClient.StartContext(context);

            while (_jsonRpcClient.NumExpectedRequests > 0 && _testStopWatch.Elapsed < TimeSpan.FromSeconds(TestTimeout))
                yield return null;

            Assert.AreEqual(_jsonRpcClient.NumExpectedRequests, 0, "Expected request was never sent by mock json rpc client");
            Assert.AreEqual(_localDataRepository.LoadDataChunksJson(TestUserId).Count(), 0, "Local repository still has items in it");
        }
        

        [Test]
        public void TestEndContext()
        {
            _coroutineManager.TestCoroutine(TestEndContextCoro());
        }

        public IEnumerator TestEndContextCoro()
        {
            yield return CoroutineManager.StartCoroutine(Authenticate());

            // Start Context
            _jsonRpcClient.ExpectRequestWithMethod(new Requests.System.BulkRequest().Method, new BooleanResponse(true));

            string testData = "test data";
            MockContext context = new MockContext(testData);
            _rDataClient.StartContext(context);

            while (_jsonRpcClient.NumExpectedRequests > 0 && _testStopWatch.Elapsed < TimeSpan.FromSeconds(TestTimeout))
                yield return null;

            Assert.AreEqual(_jsonRpcClient.NumExpectedRequests, 0, "Expected request was never sent by mock json rpc client");
            Assert.AreEqual(_localDataRepository.LoadDataChunksJson(TestUserId).Count(), 0, "Local repository still has items in it");

            // End context
            _jsonRpcClient.ExpectRequestWithMethod(new Requests.System.BulkRequest().Method, new BooleanResponse(true));
            
            _rDataClient.EndContext(context);

            while (_jsonRpcClient.NumExpectedRequests > 0 && _testStopWatch.Elapsed < TimeSpan.FromSeconds(TestTimeout))
                yield return null;

            Assert.AreEqual(_jsonRpcClient.NumExpectedRequests, 0, "Expected request was never sent by mock json rpc client");
            Assert.AreEqual(_localDataRepository.LoadDataChunksJson(TestUserId).Count(), 0, "Local repository still has items in it");
        }
    }
}
