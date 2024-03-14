using LiveTelemetrySensor.SensorAlerts.Models.Enums;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LiveTelemetrySensor.SensorAlerts.Models.LiveSensor.ValidationResults
{
    public class InvalidRangeResult : SensorValidationResult
    {
        public InvalidRangeResult(IEnumerable<string> invalidRangeParameters) : base(SensorParseStatus.INVALID_RANGE,
                    string.Format("Invalid range for {0} {1}",
                    invalidRangeParameters.Count() == 1 ? "parameter" : "parameters",
                    string.Join(", ", invalidRangeParameters)))
        {
        }
    }
}
