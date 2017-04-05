using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RData.Contexts;
using System;

namespace RData.Tests.Mock
{
    public class MockContext : RDataContext<MockContext.MockContextData>
    {
        public override int ContextDataVersion
        {
            get { return 1; }
        }

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
