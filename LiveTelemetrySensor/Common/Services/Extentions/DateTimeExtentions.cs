using System;

namespace LiveTelemetrySensor.Common.Services.Extentions
{
    public static class DateTimeExtentions
    {
        public static long ToUnix(this DateTime dateTime)
        {
            return new DateTimeOffset(dateTime).ToUnixTimeMilliseconds();
        }
    }
}
