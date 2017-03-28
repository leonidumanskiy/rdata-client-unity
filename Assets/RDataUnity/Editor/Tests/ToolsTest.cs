using NUnit.Framework;
using RData.Tools;
using System;

namespace RData.Tests
{
    [TestFixture]
    public class TimeTest
    {
        [Test]
        public void TestDateTimeToUnixTimeMilliseconds()
        {
            Assert.AreEqual(Time.DateTimeToUnixTimeMilliseconds(Convert.ToDateTime("3/28/2017, 17:14:58.00")), 1490721298000);
            Assert.AreEqual(Time.DateTimeToUnixTimeMilliseconds(Convert.ToDateTime("3/28/2017, 17:14:58.01")), 1490721298010);
            Assert.AreEqual(Time.DateTimeToUnixTimeMilliseconds(Convert.ToDateTime("3/28/2017, 17:14:58.16")), 1490721298160);
            Assert.AreEqual(Time.DateTimeToUnixTimeMilliseconds(Convert.ToDateTime("3/28/2017, 17:14:59.00")), 1490721299000);
        }

        [Test]
        public void TestDateTimeToUnixTimeSeconds()
        {
            Assert.AreEqual(Time.DateTimeToUnixTimeSeconds(Convert.ToDateTime("3/28/2017, 17:14:58.00")), 1490721298);
            Assert.AreEqual(Time.DateTimeToUnixTimeSeconds(Convert.ToDateTime("3/28/2017, 17:14:57.00")), 1490721297);
            Assert.AreEqual(Time.DateTimeToUnixTimeSeconds(Convert.ToDateTime("3/28/2017, 17:14:59.00")), 1490721299);
        }

        [Test]
        public void TestUnixTimeMillisecondsToDateTime()
        {
            Assert.AreEqual(Time.UnixTimeMillisecondsToDateTime(1490721298000), Convert.ToDateTime("3/28/2017, 17:14:58.00"));
            Assert.AreEqual(Time.UnixTimeMillisecondsToDateTime(1490721298010), Convert.ToDateTime("3/28/2017, 17:14:58.01"));
            Assert.AreEqual(Time.UnixTimeMillisecondsToDateTime(1490721298160), Convert.ToDateTime("3/28/2017, 17:14:58.16"));
            Assert.AreEqual(Time.UnixTimeMillisecondsToDateTime(1490721299000), Convert.ToDateTime("3/28/2017, 17:14:59.00"));
        }

        [Test]
        public void TestUnixTimeSecondsToDateTime()
        {
            Assert.AreEqual(Time.UnixTimeSecondsToDateTime(1490721298), Convert.ToDateTime("3/28/2017, 17:14:58.00"));
            Assert.AreEqual(Time.UnixTimeSecondsToDateTime(1490721297), Convert.ToDateTime("3/28/2017, 17:14:57.00"));
            Assert.AreEqual(Time.UnixTimeSecondsToDateTime(1490721299), Convert.ToDateTime("3/28/2017, 17:14:59.00"));
        }
    }
}
