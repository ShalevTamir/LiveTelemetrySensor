using LiveTelemetrySensor.SensorAlerts.Models;
using LiveTelemetrySensor.SensorAlerts.Models.Enums;
using LiveTelemetrySensor.Services;
using System.Collections.Generic;

namespace LiveTelemetrySensor.SensorAlerts.Services
{
    public class SensorAlertsService
    {
        private SensorPropertiesService _sensorProperties;
        private KafkaConsumerService _kafkaConsumerService;
        private TeleProcessorService _teleProcessorService;
        private RunningState _currentState;
        public SensorAlertsService(SensorPropertiesService sensorsProperties,KafkaConsumerService kafkaConsumerService, TeleProcessorService teleProcessor) 
        {
            _sensorProperties = sensorsProperties;
            _kafkaConsumerService = kafkaConsumerService;
            _teleProcessorService = teleProcessor;
            _currentState = RunningState.STOP;
            IEnumerable<LiveSensor> liveSensors = _sensorProperties.GenerateLiveSensors();
            _teleProcessorService.AddSensorsToUpdate(liveSensors);
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
            _kafkaConsumerService.StartConsumer(_teleProcessorService.ProcessTeleData);
        }

        private void StopProccessing()
        {
            _kafkaConsumerService.StopConsumer();
        }
    }
}
