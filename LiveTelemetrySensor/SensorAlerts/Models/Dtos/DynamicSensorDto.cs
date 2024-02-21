using LiveTelemetrySensor.SensorAlerts.Models.SensorDetails;
using System.Collections;
using System.Collections.Generic;

namespace LiveTelemetrySensor.SensorAlerts.Models.Dtos
{
    public class DynamicSensorDto
    {
        public IEnumerable<SensorRequirementDto> Requirements { get; set; }
    }
}
