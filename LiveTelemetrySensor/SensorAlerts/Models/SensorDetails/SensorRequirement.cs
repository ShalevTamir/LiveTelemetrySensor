using LiveTelemetrySensor.SensorAlerts.Models.Enums;
using PdfExtractor.Models.Requirement;

namespace LiveTelemetrySensor.SensorAlerts.Models.SensorDetails
{
    public class SensorRequirement
    {
        public readonly string ParameterName;
        public readonly RequirementParam Requirement;
        public readonly Duration Duration;
        public DurationStatus DurationStatus { get; set; }


        public SensorRequirement(string parameterName, RequirementParam requirement, Duration duration)
        {
            ParameterName = parameterName;
            Requirement = requirement;
            Duration = duration;
            DurationStatus = DurationStatus.REQUIREMENT_NOT_MET;
        }


    }
}
