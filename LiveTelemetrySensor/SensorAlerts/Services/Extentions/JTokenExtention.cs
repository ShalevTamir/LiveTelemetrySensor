using Newtonsoft.Json.Linq;

namespace LiveTelemetrySensor.SensorAlerts.Services.Extentions
{
    public static class JTokenExtention
    {
        public static JToken NullSafeIndexing(this JToken jToken, string index)
        {
            return ((JObject)jToken).NullSafeIndexing(index);
        }
    }
}
