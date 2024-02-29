using LiveTelemetrySensor.SensorAlerts.Models.SensorDetails;
using LiveTelemetrySensor.SensorAlerts.Services.Extentions;
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
        public DurationDto? Duration { get; set; }

        public SensorRequirement ToSensorRequirement()
        {
            return new SensorRequirement(
                ParameterName,
                Requirement.ToRequirementParam(),
                Duration?.ToDuration()
            );
        }
    }
}
