using LiveTelemetrySensor.Common.Extentions;
using LiveTelemetrySensor.Mongo.Models;
using LiveTelemetrySensor.Mongo.Services;
using LiveTelemetrySensor.Redis.Services;
using LiveTelemetrySensor.SensorAlerts.Models.Dtos;
using LiveTelemetrySensor.SensorAlerts.Models.LiveSensor;
using LiveTelemetrySensor.SensorAlerts.Models.LiveSensor.LiveSensor;
using LiveTelemetrySensor.SensorAlerts.Services.Network;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace LiveTelemetrySensor.SensorAlerts.Services
{
    public class TeleProcessorService
    {

        private CommunicationService _communicationService;
        private RedisCacheHandler _redisCacheHandler;
        private SensorsContainer _sensorsContainer;
        private MongoAlertsService _mongoAlertsService;

        public TeleProcessorService(CommunicationService communicationService, RedisCacheHandler redisCacheHandler, MongoAlertsService mongoAlertsService)
        {
            _communicationService = communicationService;
            _sensorsContainer = new SensorsContainer();
            _redisCacheHandler = redisCacheHandler;
            _mongoAlertsService = mongoAlertsService;
        }

        public IEnumerable<SensorAlertDto> GetSesnorsState()
        {
            return _sensorsContainer.GetAllSensors().Select((sensor) => new SensorAlertDto()
            {
                SensorName = sensor.SensedParamName,
                CurrentStatus = sensor.CurrentSensorState
            });
        }

        public void AddSensorsToUpdate(IEnumerable<BaseSensor> liveSensors)
        {
            foreach (var liveSensor in liveSensors)
            {
                AddSensorToUpdate(liveSensor);
            }
        }

        public void AddSensorToUpdate(BaseSensor liveSensor)
        {
            if (_sensorsContainer.hasSensor(liveSensor))
                throw new ArgumentException("Sensor " + liveSensor.SensedParamName + " already exists\n duplicate sensors are forbidden");

            _sensorsContainer.InsertSensor(liveSensor);
            _redisCacheHandler.AddRelevantRequirements(liveSensor);
        }
       
        public async Task ProcessTeleDataAsync(string JTeleData)
        {
            var telemetryFrame = JsonConvert.DeserializeObject<TelemetryFrameDto>(JTeleData);
            if (telemetryFrame == null)
                throw new ArgumentException("Unable to deserialize telemetry frame \n" + JTeleData+"");
            _redisCacheHandler.CacheTeleData(telemetryFrame);

            foreach(var dynamicSensor in _sensorsContainer.GetDynamicLiveSensors())
            {
                bool stateUpdated = dynamicSensor.Sense(_redisCacheHandler.UpdateDurationStatus);
                await handleSensorStateAsync(stateUpdated, dynamicSensor, telemetryFrame.TimeStamp);
            }

            foreach (var teleParam in telemetryFrame.Parameters)
            {
                string teleParamName = teleParam.Name.ToLower();
                if (!_sensorsContainer.hasSensor(teleParamName)) continue;
                ParameterLiveSensor parameterSensor = _sensorsContainer.GetParameterLiveSensor(teleParamName);
                bool stateUpdated = parameterSensor.Sense(double.Parse(teleParam.Value), _redisCacheHandler.UpdateDurationStatus);
                await handleSensorStateAsync(stateUpdated, parameterSensor, telemetryFrame.TimeStamp);
            }
        }

        private async Task handleSensorStateAsync(bool stateUpdated, BaseSensor sensor, DateTime timestamp)
        {
            if (stateUpdated)
            {
                await _communicationService.SendSensorAlertAsync(new SensorAlertDto()
                {
                    SensorName = sensor.SensedParamName,
                    CurrentStatus = sensor.CurrentSensorState
                });

                await _mongoAlertsService.InsertAlert(new Alert()
                {
                    SensorName = sensor.SensedParamName,
                    SensorStatus = sensor.CurrentSensorState,
                    TimeStamp = timestamp.ToUnix()
                });
                //telemetryFrame.Parameters.ToList().ForEach(p => Debug.WriteLine(p.Name + " " + p.Value));
            }
        }

       
    }
}
