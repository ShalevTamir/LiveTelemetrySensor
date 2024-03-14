using LiveTelemetrySensor.SensorAlerts.Models.Enums;
using Microsoft.AspNetCore.Mvc;

namespace LiveTelemetrySensor.SensorAlerts.Models.LiveSensor.ValidationResults
{
    public class ValidSensorResult : SensorValidationResult
    {
        public ValidSensorResult(string message = "") : base(SensorParseStatus.VALID, message)
        {
        }

        public override IActionResult ToIActionResult()
        {
            return new OkObjectResult(Message);
        }
    }
}
