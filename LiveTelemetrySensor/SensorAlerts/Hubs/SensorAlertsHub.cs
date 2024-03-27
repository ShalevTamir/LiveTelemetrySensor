using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace LiveTelemetrySensor.SensorAlerts.Hubs
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class SensorAlertsHub: Hub
    {
        public static readonly string Endpoint = "/sensor-alerts-socket";
    }
}
