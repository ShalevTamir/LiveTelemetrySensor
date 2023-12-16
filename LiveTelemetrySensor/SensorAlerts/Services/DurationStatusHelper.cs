using LiveTelemetrySensor.SensorAlerts.Models.Enums;
using System;

namespace LiveTelemetrySensor.SensorAlerts.Services
{
    public static class DurationStatusHelper
    {
        public static DurationStatus FromBool(bool value)
        {
            return (DurationStatus)Convert.ToInt32(value);
        }
    }
}
