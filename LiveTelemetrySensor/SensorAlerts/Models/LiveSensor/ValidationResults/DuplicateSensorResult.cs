using LiveTelemetrySensor.SensorAlerts.Models.Enums;

namespace LiveTelemetrySensor.SensorAlerts.Models.LiveSensor.ValidationResults
{
    public class DuplicateSensorResult : SensorValidationResult
    {
        public DuplicateSensorResult(string sensorName) : base(SensorParseStatus.DUPLICATE_SENSOR, "Sensor with name " + sensorName + " already exists, duplicate sensors are forbidden")
        {
        }
    }
}
