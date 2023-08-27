using LiveTelemetrySensor.SensorAlerts.Models.Dtos;
using Newtonsoft.Json;
using System.Diagnostics;

namespace LiveTelemetrySensor.SensorAlerts.Services.Network
{
    public class CommunicationService
    {
        public void SendSensorAlert(SensorAlertDto sensorAlertDto) 
        { 
            Debug.WriteLine(JsonConvert.SerializeObject(sensorAlertDto,Formatting.Indented));
        }
    }
}
