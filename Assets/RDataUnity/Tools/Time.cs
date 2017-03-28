using System;
using System.Collections;

namespace RData.Tools
{
    public static class Time
    {
        /// <summary>
        /// Returns number of milliseconds passed since Unix Epoch
        /// Equivalent for JavaScript Date.now() function
        /// </summary>
        /// <returns>Unix Time</returns>
        public static long UnixTimeMilliseconds
        {
            get { return DateTimeToUnixTimeMilliseconds(DateTime.UtcNow); }
        }

        /// <summary>
        /// Returns number of seconds passed since Unix Epoch
        /// </summary>
        /// <returns>Unix Time</returns>
        public static long UnixTimeSeconds
        {
            get { return DateTimeToUnixTimeSeconds(DateTime.UtcNow); }
        }

        /// <summary>
        /// Converts System.DateTime into long that contains number 
        /// of milliseconds passed since January 1st, 1970 in UTC
        /// </summary>
        /// <param name="dateTime">DateTime to convert</param>
        /// <returns>Unix Time</returns>
        public static long DateTimeToUnixTimeMilliseconds(DateTime dateTime)
        {
            return
                (long)
                    dateTime.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }

        /// <summary>
        /// Converts unix timestamp (long number that represnets number of
        /// of milliseconds passed since January 1st, 1970 in UTC) to DateTime
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        public static DateTime UnixTimeMillisecondsToDateTime(long timeStamp)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc) + TimeSpan.FromMilliseconds(timeStamp);
        }

        /// <summary>
        /// Converts System.DateTime into long that contains number 
        /// of seconds passed since January 1st, 1970 in UTC
        /// </summary>
        /// <param name="dateTime">DateTime to convert</param>
        /// <returns>Unix Time</returns>
        public static long DateTimeToUnixTimeSeconds(DateTime dateTime)
        {
            return
                (long)
                    dateTime.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }

        /// <summary>
        /// Converts unix timestamp (long number that represnets number of
        /// of milliseconds passed since January 1st, 1970 in UTC) to DateTime
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        public static DateTime UnixTimeSecondsToDateTime(long timeStamp)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc) + TimeSpan.FromSeconds(timeStamp);
        }
    }
}