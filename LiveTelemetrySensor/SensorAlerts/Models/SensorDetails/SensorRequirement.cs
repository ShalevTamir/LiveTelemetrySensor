using LiveTelemetrySensor.SensorAlerts.Models.Enums;
using PdfExtractor.Models.Requirement;

namespace LiveTelemetrySensor.SensorAlerts.Models.SensorDetails
{
    public class SensorRequirement
    {
        public readonly string ParameterName;
        public readonly RequirementParam Requirement;
        public readonly Duration? Duration;
        public RequirementStatus RequirementStatus { get; set; }


        public SensorRequirement(string parameterName, RequirementParam requirement, Duration? duration = null)
        {
            ParameterName = parameterName.ToLower();
            Requirement = requirement;
            Duration = duration;
            RequirementStatus = RequirementStatus.REQUIREMENT_NOT_MET;
        }


    }
}
