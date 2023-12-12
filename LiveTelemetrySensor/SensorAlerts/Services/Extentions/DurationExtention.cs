using LiveTelemetrySensor.SensorAlerts.Models.Enums;
using LiveTelemetrySensor.SensorAlerts.Models.SensorDetails;
using PdfExtractor.Models.Requirement;

namespace LiveTelemetrySensor.SensorAlerts.Services.Extentions
{
    public static class DurationExtention
    {
        // Converts to Epoch Time, with null indicating an infinite amount
        public static long? RetentionTime(this Duration duration)
        {
            RequirementParam requirement = duration.RequirementParam;
            if (requirement is RequirementRange)
            {
                RequirementRange range = (RequirementRange)requirement;
                // value: numeric, end_value: inf
                if (range.EndValue == double.PositiveInfinity)
                    return null;
                // value: -inf, end_value: numeric
                // value: numeric, end_value: numeric
                return duration.DurationType.ToMillis(range.EndValue);

            }
            else
            {
                return duration.DurationType.ToMillis(requirement.Value);
            }
        }
    }
}
