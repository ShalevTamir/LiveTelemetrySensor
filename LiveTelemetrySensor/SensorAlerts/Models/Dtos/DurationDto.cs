using LiveTelemetrySensor.SensorAlerts.Models.Enums;
using LiveTelemetrySensor.SensorAlerts.Models.SensorDetails;
using LiveTelemetrySensor.SensorAlerts.Services.Extentions;

namespace LiveTelemetrySensor.SensorAlerts.Models.Dtos
{
    public class DurationDto
    {
        public DurationType DurationType { get; set; }
        public RequirementDto Requirement { get; set; }

        public DurationDto()
        {

        }

        public DurationDto(DurationType durationType, RequirementDto requirement)
        {
            DurationType = durationType;
            Requirement = requirement;
        }   

        public DurationDto(Duration duration)
        {
            DurationType = duration.DurationType;
            Requirement = new RequirementDto(duration.Requirement);
        }
        public Duration ToDuration()
        {
            return new Duration(DurationType, Requirement.ToRequirementParam());
        }
    }
}
