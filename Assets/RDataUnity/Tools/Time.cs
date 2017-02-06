using System;
using System.Collections;

namespace RData.Tools
{
    public static class Time
    {
        /// <summary>
        /// Returns number of milliseconds
        /// Equivalent for JavaScript Date.now() function
        /// </summary>
        /// <returns>Unix Time</returns>
        public static long UnixTime
        {
            get { return DateTimeToUnixTime(DateTime.UtcNow); }
        }

        /// <summary>
        /// Converts System.DateTime into long that contains number 
        /// of milliseconds passed since January 1st, 1970 in UTC
        /// </summary>
        /// <param name="dateTime">DateTime to convert</param>
        /// <returns>Unix Time</returns>
        public static long DateTimeToUnixTime(DateTime dateTime)
        {
            return
                (long)
                    DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }

        /// <summary>
        /// Converts unix timestamp (long number that represnets number of
        /// of milliseconds passed since January 1st, 1970 in UTC) to DateTime
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        public static DateTime UnixTimeToDateTime(long timeStamp)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc) + TimeSpan.FromMilliseconds(timeStamp);
        }
    }
}