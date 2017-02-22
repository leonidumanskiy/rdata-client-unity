using System.Collections;
using System.Collections.Generic;
using RData.JsonRpc;
using RData.Responses;

namespace RData.Tests.Mock
{
    public class MockRequest : JsonRpcRequest<MockRequest.Parameters, BooleanResponse>
    {
        public override string Method
        {
            get { return "test"; }
        }

        public class Parameters
        {
            [LitJson.JsonAlias("test")]
            public string Test { get; set; }
        }

        public MockRequest(string test)
        {
            Params = new Parameters()
            {
                Test = test
            };
        }
    }
}