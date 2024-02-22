using LiveTelemetrySensor.SensorAlerts.Models.SensorDetails;
using Newtonsoft.Json;
using PdfExtractor.Models.Requirement;
using System.Text.Json.Serialization;

namespace LiveTelemetrySensor.SensorAlerts.Models.Dtos
{
    public class SensorRequirementDto
    {
        public string ParameterName { get; set; }
        public RequirementDto Requirement { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Duration? Duration { get; set; }
    }
}
