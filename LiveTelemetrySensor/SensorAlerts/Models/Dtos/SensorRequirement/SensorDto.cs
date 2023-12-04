using PdfExtractor.Models.Requirement;

namespace LiveTelemetrySensor.SensorAlerts.Models.Dtos.SensorRequirement
{
    public class SensorDto
    {
        public string parameterName;
        public RequirementParam requirementParam;
        public DurationDto duration = null;
    }
}
