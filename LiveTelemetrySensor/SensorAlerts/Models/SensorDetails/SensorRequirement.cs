using LiveTelemetrySensor.SensorAlerts.Models.Dtos.SensorRequirement;
using PdfExtractor.Models.Requirement;

namespace LiveTelemetrySensor.SensorAlerts.Models.SensorDetails
{
    public class SensorRequirement
    {
        public readonly string ParameterName;
        public readonly RequirementParam Requirement;
        public readonly Duration Duration;

        public SensorRequirement(string parameterName, RequirementParam requirement, Duration duration)
        {
            ParameterName = parameterName;
            Requirement = requirement;
            Duration = duration;
        }


    }
}
