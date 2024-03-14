using LiveTelemetrySensor.SensorAlerts.Models.Enums;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LiveTelemetrySensor.SensorAlerts.Models.LiveSensor.ValidationResults
{
    public class UnkownParametersResult : SensorValidationResult
    {
        public UnkownParametersResult(IEnumerable<string> unkownParameterNames) : base(SensorParseStatus.UNKOWN_PARAMETERS,
                    string.Format("{0} {1} {2} not recognized",
                    unkownParameterNames.Count() == 1 ? "Parameter" : "Parameters",
                    string.Join(", ", unkownParameterNames),
                    unkownParameterNames.Count() == 1 ? "is" : "are"))
        {
        }
    }
}
