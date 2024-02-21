using LiveTelemetrySensor.SensorAlerts.Models.Enums;
using LiveTelemetrySensor.SensorAlerts.Models.SensorDetails;
using PdfExtractor.Models.Requirement;

namespace LiveTelemetrySensor.SensorAlerts.Services.Extentions
{
    public static class DurationExtention
    {
       

        // Converts to Epoch Time, with null indicating an infinite amount
        public static long RetentionTime(this Duration duration, RequirementTime requirementTime = RequirementTime.MAXIMUM, long retentionMargin = 0)
        {
            RequirementParam requirement = duration.Requirement;
            long retentionTime;
            if (requirement is RequirementRange requirementRange)
            {
                // value: numeric, end_value: inf
                if (requirementRange.EndValue == double.PositiveInfinity)
                    retentionTime = duration.DurationType.ToMillis(requirementRange.Value);
                // value: -inf, end_value: numeric
                else if(requirementRange.Value == double.NegativeInfinity) 
                    retentionTime = duration.DurationType.ToMillis(requirementRange.EndValue);
                // value: numeric, end_value: numeric
                else
                    retentionTime = 
                            requirementTime == RequirementTime.MAXIMUM ?
                            duration.DurationType.ToMillis(requirementRange.EndValue) :
                            duration.DurationType.ToMillis(requirementRange.Value);
            }
            else
            {
                retentionTime = duration.DurationType.ToMillis(requirement.Value);
            }
            return retentionTime + retentionMargin;
        }
    }
}
