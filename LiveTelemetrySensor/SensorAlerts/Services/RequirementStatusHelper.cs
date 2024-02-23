using LiveTelemetrySensor.SensorAlerts.Models.Enums;
using System;

namespace LiveTelemetrySensor.SensorAlerts.Services
{
    public static class RequirementStatusHelper
    {
        public static RequirementStatus FromBool(bool value)
        {
            return (RequirementStatus)Convert.ToInt32(value);
        }
    }
}
