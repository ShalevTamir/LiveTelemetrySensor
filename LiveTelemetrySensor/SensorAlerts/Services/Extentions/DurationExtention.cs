using LiveTelemetrySensor.SensorAlerts.Models.Enums;
using LiveTelemetrySensor.SensorAlerts.Models.SensorDetails;
using PdfExtractor.Models.Requirement;

namespace LiveTelemetrySensor.SensorAlerts.Services.Extentions
{
    public static class DurationExtention
    {
        // Converts to Epoch Time, with null indicating an infinite amount
        public static long RetentionTime(this Duration duration, long retentionMargin = 0)
        {
            RequirementParam requirement = duration.RequirementParam;
            long retentionTime;
            if (requirement is RequirementRange)
            {
                RequirementRange range = (RequirementRange)requirement;
                // value: numeric, end_value: inf
                if (range.EndValue == double.PositiveInfinity)
                    retentionTime = duration.DurationType.ToMillis(requirement.Value);
                // value: -inf, end_value: numeric
                // value: numeric, end_value: numeric
                else
                    retentionTime = duration.DurationType.ToMillis(range.EndValue);

            }
            else
            {
                retentionTime = duration.DurationType.ToMillis(requirement.Value);
            }
            return retentionTime + retentionMargin;
        }
    }
}
