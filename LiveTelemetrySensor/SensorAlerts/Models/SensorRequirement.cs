using LiveTelemetrySensor.SensorAlerts.Models.Dtos.SensorRequirement;
using PdfExtractor.Models.Requirement;

namespace LiveTelemetrySensor.SensorAlerts.Models
{
    public class SensorRequirement
    {
        public readonly string ParameterName;
        public readonly RequirementParam Requirement;

        public SensorRequirement(string parameterName, RequirementParam requirement)
        {
            ParameterName = parameterName;
            Requirement = requirement;
        }


    }
}
