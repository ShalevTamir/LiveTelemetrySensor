using PdfExtractor.Models.Requirement;

namespace LiveTelemetrySensor.SensorAlerts.Models.Dtos.SensorRequirement
{
    public class SensorDto
    {
        public string parameter_name { get; set; }
        public RequirementParamDto requirement_param { get; set; }
        public DurationDto duration { get; set; }
    }
}
