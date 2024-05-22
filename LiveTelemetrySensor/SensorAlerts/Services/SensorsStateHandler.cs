using LiveTelemetrySensor.Mongo.Services;
using LiveTelemetrySensor.SensorAlerts.Models.Dtos;
using LiveTelemetrySensor.SensorAlerts.Models.LiveSensor;
using LiveTelemetrySensor.SensorAlerts.Models.LiveSensor.LiveSensor;
using LiveTelemetrySensor.SensorAlerts.Services.Network;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using LiveTelemetrySensor.Mongo.Models;

namespace LiveTelemetrySensor.SensorAlerts.Services
{
    public class SensorsStateHandler
    {
        private SensorsContainer _sensorsContainer;
        private CommunicationService _communicationService;
        private SensorValidator _sensorValidator;
        private MongoAlertsService _mongoAlertsService;
        public SensorsStateHandler(
            SensorsContainer sensorsContainer,
            CommunicationService communicationService,
            SensorValidator sensorValidator,
            MongoAlertsService mongoAlertsService) 
        {
            _sensorsContainer = sensorsContainer;
            _communicationService = communicationService;
            _sensorValidator = sensorValidator;
            _mongoAlertsService = mongoAlertsService;
        }

        public async Task UpdateParameterSensorsAsync(IEnumerable<TelemetryParameterDto> parameters)
        {
            foreach (var teleParam in parameters)
            {
                var validation = _sensorValidator.CheckParameterSensorExists(teleParam.Name);
                if (validation.IsValid())
                {
                    ParameterLiveSensor parameterSensor = _sensorsContainer.GetParameterLiveSensor(teleParam.Name);
                    bool stateUpdated = parameterSensor.Sense(double.Parse(teleParam.Value));
                    await handleSensorStateAsync(stateUpdated, parameterSensor);
                }
            }
        }

        public async Task UpdateDynamicSensorsAsync()
        {
            foreach (var dynamicSensor in _sensorsContainer.GetDynamicLiveSensors())
            {
                bool stateUpdated = dynamicSensor.Sense();
                await handleSensorStateAsync(stateUpdated, dynamicSensor);
            }
        }

        private async Task handleSensorStateAsync(bool stateUpdated, BaseSensor sensor)
        {
            if (stateUpdated)
            {
                await _communicationService.SendSensorAlertAsync(new SensorAlertDto()
                {
                    SensorName = sensor.SensedParamName,
                    CurrentStatus = sensor.CurrentSensorState
                });
                _mongoAlertsService.AddAlertToFrame(new Alert()
                {
                    SensorName = sensor.SensedParamName,
                    SensorStatus = sensor.CurrentSensorState
                });
                    
            }
        }


    }
}
