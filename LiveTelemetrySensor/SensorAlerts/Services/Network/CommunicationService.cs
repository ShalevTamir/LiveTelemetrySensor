using LiveTelemetrySensor.SensorAlerts.Hubs;
using LiveTelemetrySensor.SensorAlerts.Models.Dtos;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Threading.Tasks;

namespace LiveTelemetrySensor.SensorAlerts.Services.Network
{
    public class CommunicationService
    {
        private IHubContext<SensorAlertsHub> _hubContext;

        public CommunicationService(IHubContext<SensorAlertsHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendSensorAlertAsync(SensorAlertDto sensorAlertDto) 
        {
            Debug.WriteLine(JsonConvert.SerializeObject(sensorAlertDto,Formatting.Indented));
            await _hubContext.Clients.All.SendAsync("receiveAlerts", sensorAlertDto);
        }
    }
}
