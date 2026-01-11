using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContangoPortal.Service
{
    public static class DateTimeExtensions
    {
        private static readonly TimeZoneInfo IndiaTimeZone =
            TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

        public static DateTime ToIndiaTime(this DateTime utcDateTime)
        {
            if (utcDateTime.Kind != DateTimeKind.Utc)
                utcDateTime = utcDateTime.ToUniversalTime();

            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, IndiaTimeZone);
        }
    }
}