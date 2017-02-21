using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RData.Contexts;

namespace RData.Tests.Mock
{
    public class MockContext : RDataContext<MockContext.MockContextData>
    {
        public class MockContextData
        {
            public string TestData;
        }

        public MockContext(string testData)
            : base(new MockContextData() { TestData = testData })
        {
        }
    }
}
