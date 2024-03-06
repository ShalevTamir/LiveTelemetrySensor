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

        public TeleProcessorService(CommunicationService communicationService, RedisCacheHandler redisCacheHandler, MongoAlertsService mongoAlertsService, SensorsContainer sensorsContainer)
        {
            _communicationService = communicationService;
            _sensorsContainer = sensorsContainer;
            _redisCacheHandler = redisCacheHandler;
            _mongoAlertsService = mongoAlertsService;
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

        public void RemoveSensorToUpdate(string sensorName)
        {
            bool removed = _sensorsContainer.RemoveSensor(sensorName);
            if (!removed)
                throw new ArgumentException("Sensor " + sensorName + " doesn't exist");
        }
       
        public async Task ProcessTeleDataAsync(string JTeleData)
        {
            var telemetryFrame = JsonConvert.DeserializeObject<TelemetryFrameDto>(JTeleData);
            //TODO: not don't exception
            if (telemetryFrame == null)
                throw new ArgumentException("Unable to deserialize telemetry frame \n" + JTeleData+"");

            lowerCaseParameterNames(telemetryFrame);
            List<Alert> alertsInFrame = new List<Alert>();
            _redisCacheHandler.CacheTeleData(telemetryFrame);
            foreach(var sensor in _sensorsContainer.GetAllSensors())
            {
                sensor.UpdateAdditionalRequirementStatus(_redisCacheHandler.UpdateRequirementStatus);
            }

            foreach(var dynamicSensor in _sensorsContainer.GetDynamicLiveSensors())
            {
                bool stateUpdated = dynamicSensor.Sense();
                await handleSensorStateAsync(stateUpdated, dynamicSensor, alertsInFrame);
            }

            foreach (var teleParam in telemetryFrame.Parameters)
            {
                if (_sensorsContainer.hasSensor(teleParam.Name)){
                    ParameterLiveSensor parameterSensor = _sensorsContainer.GetParameterLiveSensor(teleParam.Name);
                    bool stateUpdated = parameterSensor.Sense(double.Parse(teleParam.Value));
                    await handleSensorStateAsync(stateUpdated, parameterSensor, alertsInFrame);
                }
            }
            if (alertsInFrame.Count > 0)
            {
                await _mongoAlertsService.InsertAlerts(new Alerts()
                {
                    TimeStamp = telemetryFrame.TimeStamp.ToUnix(),
                    MongoAlerts = alertsInFrame
                });
            }
        }

        private async Task handleSensorStateAsync(bool stateUpdated, BaseSensor sensor, List<Alert> alertsInFrame)
        {
            if (stateUpdated)
            {
                await _communicationService.SendSensorAlertAsync(new SensorAlertDto()
                {
                    SensorName = sensor.SensedParamName,
                    CurrentStatus = sensor.CurrentSensorState
                });

                alertsInFrame.Add(
                    new Alert()
                    {
                        SensorName = sensor.SensedParamName,
                        SensorStatus = sensor.CurrentSensorState,

                    });

                //telemetryFrame.Parameters.ToList().ForEach(p => Debug.WriteLine(p.Name + " " + p.Value));
            }
        }
        
        private void lowerCaseParameterNames(TelemetryFrameDto frame)
        {
            foreach(var parameter in frame.Parameters) 
            {
                parameter.Name = parameter.Name.ToLower();
            }
        }
       
    }
}
