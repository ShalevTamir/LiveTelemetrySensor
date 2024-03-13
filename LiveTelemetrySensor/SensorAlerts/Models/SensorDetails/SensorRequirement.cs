using LiveTelemetrySensor.SensorAlerts.Models.Dtos;
using LiveTelemetrySensor.SensorAlerts.Models.Enums;
using LiveTelemetrySensor.SensorAlerts.Services.Extentions;
using Newtonsoft.Json;
using PdfExtractor.Models.Requirement;

namespace LiveTelemetrySensor.SensorAlerts.Models.SensorDetails
{
    public class SensorRequirement
    {
        public readonly string ParameterName;
        public readonly RequirementParam Requirement;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public readonly Duration? Duration;

        [JsonIgnore]
        public RequirementStatus RequirementStatus { get; set; }


        public SensorRequirement(string parameterName, RequirementParam requirement, Duration? duration = null)
        {
            ParameterName = parameterName.ToLower();
            Requirement = requirement;
            Duration = duration;
            RequirementStatus = RequirementStatus.REQUIREMENT_NOT_MET;
        }

        public bool IsValid()
        {
            return Requirement.IsValid() && (Duration == null || Duration.Requirement.IsValid());
        }

        public SensorRequirementDto ToRequirementDto()
        {
            var sensorRequirementDto = new SensorRequirementDto
            {
                ParameterName = ParameterName,
                Requirement = new RequirementDto(Requirement),

            };
            if (Duration != null)
            {
                sensorRequirementDto.Duration = new DurationDto(Duration);
            }
            return sensorRequirementDto;
        }

    }
}
