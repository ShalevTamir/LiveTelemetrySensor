using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LiveTelemetrySensor.SensorAlerts.Models.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SensorState { NEUTRAL, VALID, NORMAL,INVALID }
}
