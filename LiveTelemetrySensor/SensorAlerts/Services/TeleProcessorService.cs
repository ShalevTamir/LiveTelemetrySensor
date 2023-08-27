using LiveTelemetrySensor.SensorAlerts.Models;
using LiveTelemetrySensor.SensorAlerts.Models.Dtos;
using LiveTelemetrySensor.SensorAlerts.Services.Network;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace LiveTelemetrySensor.SensorAlerts.Services
{
    public class TeleProcessorService
    {
        private Dictionary<string,LiveSensor> _liveSensors;
        private CommunicationService _communicationService;

        public TeleProcessorService(CommunicationService communicationService)
        {
            _communicationService = communicationService;
            _liveSensors = new Dictionary<string, LiveSensor>();
        }
        public void AddSensorsToUpdate(IEnumerable<LiveSensor> liveSensors)
        {
            foreach (var liveSensor in liveSensors)
                _liveSensors.Add(liveSensor.SensedParamName.ToLower(),liveSensor);
        }
       
        public void ProcessTeleData(string JTeleData)
        {
            var telemetryFrame = JsonConvert.DeserializeObject<TelemetryFrameDto>(JTeleData);

            foreach(var teleParam in telemetryFrame.Parameters)
            {
                string teleParamName = teleParam.Name.ToLower();
                if (!_liveSensors.ContainsKey(teleParamName)) continue;
                LiveSensor liveSensor = _liveSensors[teleParamName];
                bool stateUpdated = liveSensor.Sense(teleParam.Value);
                if (stateUpdated)
                    _communicationService.SendSensorAlert(new SensorAlertDto()
                    {
                        SensorName = liveSensor.SensedParamName,
                        CurrentStatus = liveSensor.CurrentSensorState
                    });
            }
        }

       
    }
}
