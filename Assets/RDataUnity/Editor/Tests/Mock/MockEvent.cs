using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RData.Events;

namespace RData.Tests.Mock
{
    public class MockEvent : RDataEvent<MockEvent.MockEventData>
    {
        public override int EventDataVersion
        {
            get { return 1; }
        }

        public class MockEventData
        {
            public string TestData;
        }

        public MockEvent(string testData)
            : base(new MockEventData() { TestData = testData })
        {
        }
    }
}
