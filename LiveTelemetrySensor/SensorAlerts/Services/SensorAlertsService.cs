using LiveTelemetrySensor.SensorAlerts.Models.Enums;
using LiveTelemetrySensor.Consumer.Services;
using System.Collections.Generic;
using LiveTelemetrySensor.SensorAlerts.Models.LiveSensor;
using LiveTelemetrySensor.SensorAlerts.Models.Dtos;
using LiveTelemetrySensor.SensorAlerts.Models.LiveSensor.LiveSensor;

namespace LiveTelemetrySensor.SensorAlerts.Services
{
    public class SensorAlertsService
    {
        private LiveSensorFactory _liveSensorFactory;
        private KafkaConsumerService _kafkaConsumerService;
        private TeleProcessorService _teleProcessorService;
        private RunningState _currentState;
        public SensorAlertsService(LiveSensorFactory sensorsProperties,KafkaConsumerService kafkaConsumerService, TeleProcessorService teleProcessor) 
        {
            _liveSensorFactory = sensorsProperties;
            _kafkaConsumerService = kafkaConsumerService;
            _teleProcessorService = teleProcessor;
            _currentState = RunningState.STOP;
            IEnumerable<BaseSensor> liveSensors = _liveSensorFactory.BuildLiveSensors();
            _teleProcessorService.AddSensorsToUpdate(liveSensors);
        }

        public IEnumerable<SensorAlertDto> GetSensorsState()
        {
            return _teleProcessorService.GetSesnorsState();
        }

        public DynamicLiveSensor AddDirectSensor(string sensorName, string additionalRequirements)
        {
            DynamicLiveSensor sensor = (DynamicLiveSensor)_liveSensorFactory.BuildLiveSensor(sensorName, additionalRequirements);
            _teleProcessorService.AddSensorToUpdate(sensor);
            return sensor;
        }

        public bool ChangeState(RunningState stateToChangeTo)
        {
            if(_currentState == stateToChangeTo) return false;
            switch(stateToChangeTo)
            {
                case RunningState.START:
                    StartProccessing();
                    break;
                case RunningState.STOP:
                    StopProccessing();
                    break;
            }
            _currentState = stateToChangeTo;
            return true;
        }

       
        private void StartProccessing()
        {
            _kafkaConsumerService.StartConsumer(_teleProcessorService.ProcessTeleDataAsync);
        }

        private void StopProccessing()
        {
            _kafkaConsumerService.StopConsumer();
        }
    }
}
