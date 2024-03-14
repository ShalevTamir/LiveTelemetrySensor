using LiveTelemetrySensor.SensorAlerts.Models.Enums;

namespace LiveTelemetrySensor.SensorAlerts.Models.LiveSensor.ValidationResults
{
    public class DoesntExistResult : SensorValidationResult
    {
        public DoesntExistResult(string sensorName) : base(SensorParseStatus.DOESNT_EXIST, "Sensor " + sensorName + "doesn't exist")
        {
        }
    }
}
