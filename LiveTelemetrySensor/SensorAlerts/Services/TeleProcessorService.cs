using LiveTelemetrySensor.Common.Extentions;
using LiveTelemetrySensor.Mongo.Models;
using LiveTelemetrySensor.Mongo.Services;
using LiveTelemetrySensor.Redis.Services;
using LiveTelemetrySensor.SensorAlerts.Models.Dtos;
using LiveTelemetrySensor.SensorAlerts.Models.Enums;
using LiveTelemetrySensor.SensorAlerts.Models.LiveSensor;
using LiveTelemetrySensor.SensorAlerts.Models.LiveSensor.LiveSensor;
using LiveTelemetrySensor.SensorAlerts.Models.LiveSensor.ValidationResults;
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
        private SensorValidator _sensorValidator;
        private SensorsStateHandler _sensorsStateHandler;

        public TeleProcessorService(
            CommunicationService communicationService,
            RedisCacheHandler redisCacheHandler,
            MongoAlertsService mongoAlertsService,
            SensorsContainer sensorsContainer,
            SensorValidator sensorValidator,
            SensorsStateHandler sensorsStateHandler)
        {
            _communicationService = communicationService;
            _sensorsContainer = sensorsContainer;
            _redisCacheHandler = redisCacheHandler;
            _mongoAlertsService = mongoAlertsService;
            _sensorValidator = sensorValidator;
            _sensorsStateHandler = sensorsStateHandler;
        }

        public SensorValidationResult AddSensorsToUpdate(IEnumerable<BaseSensor> liveSensors)
        {
            var validationResults = liveSensors.Select(sensor => AddSensorToUpdate(sensor));
            var invalidResult = validationResults.FirstOrDefault(validationResult => !validationResult.IsValid());
            return invalidResult == null ? new ValidSensorResult() : invalidResult;
        }

        public SensorValidationResult AddSensorToUpdate(BaseSensor liveSensor)
        {
            var nameValidation = _sensorValidator.SensorNameValidation(liveSensor.SensedParamName);
            if (nameValidation.IsValid())
            {
                _sensorsContainer.InsertSensor(liveSensor);
                _redisCacheHandler.AddRelevantRequirements(liveSensor);
            }
            return nameValidation;

        }

        public SensorValidationResult RemoveSensorToUpdate(string sensorName)
        {
            var validation = _sensorValidator.CheckSensorExists(sensorName);
            if (validation.IsValid())
            {
                _sensorsContainer.RemoveSensor(sensorName);
            }
            return validation;
        }
       
        public async Task ProcessTeleDataAsync(string JTeleData)
        {
            var telemetryFrame = JsonConvert.DeserializeObject<TelemetryFrameDto>(JTeleData);
            if (telemetryFrame != null)
            {
                LowerCaseParameterNames(telemetryFrame);
                _redisCacheHandler.ProcessFrame(telemetryFrame);
                _mongoAlertsService.OpenNewFrame();
                await _sensorsStateHandler.UpdateDynamicSensorsAsync();
                await _sensorsStateHandler.UpdateParameterSensorsAsync(telemetryFrame.Parameters);
                await _mongoAlertsService.InsertCurrentFrameAsync(telemetryFrame.TimeStamp);
            }
            else
            {
                Debug.WriteLine("Unable to deserialize frame " + JTeleData);
            }

        }
        
        private void LowerCaseParameterNames(TelemetryFrameDto frame)
        {
            foreach (var parameter in frame.Parameters) 
            {
                parameter.Name = parameter.Name.ToLower();
            }
        }
       
    }
}
