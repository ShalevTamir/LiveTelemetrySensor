using Newtonsoft.Json;
using PdfExtractor.Models.Requirement;

namespace LiveTelemetrySensor.SensorAlerts.Models.Dtos
{
    public class RequirementDto
    {
        public string Value { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string? EndValue { get; set; }

        public RequirementDto() { }
        public RequirementDto(RequirementParam requirement) 
        {
            Value = ParseValue(requirement.Value);
            if(requirement is RequirementRange requirementRange)
            {
                EndValue = ParseValue(requirementRange.EndValue);
            }
        }

        private string ParseValue(double value)
        {
            if (value == double.PositiveInfinity) return "Infinity";
            if (value == double.NegativeInfinity) return "-Infinity";
            return value.ToString();
        }
    }
}
