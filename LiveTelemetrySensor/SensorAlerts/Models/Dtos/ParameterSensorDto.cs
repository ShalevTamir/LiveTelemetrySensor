using LiveTelemetrySensor.SensorAlerts.Models.LiveSensor.LiveSensor;
using LiveTelemetrySensor.SensorAlerts.Services.Extentions;
using PdfExtractor.Models.Requirement;
using System.Linq;

namespace LiveTelemetrySensor.SensorAlerts.Models.Dtos
{
    public class ParameterSensorDto
    {
        public string SensorName { get; set; }
        public RequirementModelDto[] Requirements { get; set; }
        public SensorRequirementDto[] AdditionalRequirements { get; set; }

        public ParameterLiveSensor ToLiveSensor()
        {
            return new ParameterLiveSensor(
                SensorName,
                AdditionalRequirements.Select(sensorRequirementDto => sensorRequirementDto.ToSensorRequirement()).ToArray(),
                Requirements.Select(requirementDto => requirementDto.ToRequirementModel()).ToArray()
                );
        }
    }
}
