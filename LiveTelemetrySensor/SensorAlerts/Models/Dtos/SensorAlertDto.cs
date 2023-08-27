using LiveTelemetrySensor.SensorAlerts.Models.Enums;

namespace LiveTelemetrySensor.SensorAlerts.Models.Dtos
{
    public class SensorAlertDto
    {
        public string SensorName { get; set; }
        public SensorState CurrentStatus { get; set; }
    }
}
