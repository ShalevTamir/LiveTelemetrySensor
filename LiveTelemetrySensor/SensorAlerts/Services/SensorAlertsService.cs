using LiveTelemetrySensor.SensorAlerts.Models.Enums;
using LiveTelemetrySensor.Consumer.Services;
using System.Collections.Generic;
using LiveTelemetrySensor.SensorAlerts.Models.LiveSensor;
using LiveTelemetrySensor.SensorAlerts.Models.Dtos;
using LiveTelemetrySensor.SensorAlerts.Models.LiveSensor.LiveSensor;
using System.Threading.Tasks;
using System.Linq;

namespace LiveTelemetrySensor.SensorAlerts.Services
{
    public class SensorAlertsService
    {
        private KafkaConsumerService _kafkaConsumerService;
        private TeleProcessorService _teleProcessorService;
        private RunningState _currentState;
        public SensorAlertsService(KafkaConsumerService kafkaConsumerService, TeleProcessorService teleProcessor) 
        {
            _kafkaConsumerService = kafkaConsumerService;
            _teleProcessorService = teleProcessor;
            _currentState = RunningState.STOP;
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
