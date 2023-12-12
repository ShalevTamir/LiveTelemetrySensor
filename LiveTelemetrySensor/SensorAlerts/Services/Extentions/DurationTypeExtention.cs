using LiveTelemetrySensor.SensorAlerts.Models.Enums;
using LiveTelemetrySensor.SensorAlerts.Models.SensorDetails;

namespace LiveTelemetrySensor.SensorAlerts.Services.Extentions
{
    public static class DurationTypeExtention
    {
        public static long ToMillis(this DurationType durationType, double durationLength)
        {
            return (long)(durationLength * durationType.MillisMultipler());
        }

        public static long MillisMultipler(this DurationType durationType)
        {
            switch (durationType)
            {
                case DurationType.SECONDS:
                    return 1000;
                case DurationType.MINUTES:
                    return 60_000;
                case DurationType.HOURS:
                    return 3_600_000;
                default:
                    return 0;
            }
            
        }
    }
}
