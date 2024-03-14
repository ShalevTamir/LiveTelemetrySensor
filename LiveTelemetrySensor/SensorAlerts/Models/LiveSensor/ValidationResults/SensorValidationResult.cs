using LiveTelemetrySensor.SensorAlerts.Models.Enums;
using Microsoft.AspNetCore.Mvc;

namespace LiveTelemetrySensor.SensorAlerts.Models.LiveSensor.ValidationResults
{
    public class SensorValidationResult
    {
        public SensorParseStatus Status { get; set; }
        public string Message { get; set; }

        public SensorValidationResult(SensorParseStatus status, string message = "")
        {
            Status = status;
            Message = message;
        }

        public bool IsValid()
        {
            return Status == SensorParseStatus.VALID;
        }

        public void SetValidMessage(string message)
        {
            if (IsValid())
            {
                Message = message;
            }
        }

        public virtual IActionResult ToIActionResult()
        {
            return new BadRequestObjectResult(Message);
        }
    }
}
