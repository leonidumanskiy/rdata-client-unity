using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RData.Events;

namespace RData.Tests.Mock
{
    public class MockEvent : RDataEvent<MockEvent.MockEventData>
    {
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
