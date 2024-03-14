using LiveTelemetrySensor.SensorAlerts.Models.Enums;

namespace LiveTelemetrySensor.SensorAlerts.Models.LiveSensor.ValidationResults
{
    public class EmptyRequirementsResult : SensorValidationResult
    {
        public EmptyRequirementsResult(string message = "") : base(SensorParseStatus.EMPTY_REQUIREMENTS, message)
        {
        }
    }
}
