using LiveTelemetrySensor.SensorAlerts.Models.Enums;
using LiveTelemetrySensor.SensorAlerts.Models.LiveSensor;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LiveTelemetrySensor.SensorAlerts.Services
{
    public class StartupService : IHostedService
    {
        private LiveSensorFactory _liveSensorFactory;
        private TeleProcessorService _teleProcessorService;
        private SensorAlertsService _sensorAlertsService;

        public StartupService(LiveSensorFactory liveSensorFactory, TeleProcessorService teleProcessorService, SensorAlertsService sensorAlertsService)
        {
            _liveSensorFactory = liveSensorFactory;
            _teleProcessorService = teleProcessorService;
            _sensorAlertsService = sensorAlertsService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            //IEnumerable<BaseSensor> liveSensors = await _liveSensorFactory.BuildDefaultParameterSensorsAsync();
            //_teleProcessorService.AddSensorsToUpdate(liveSensors);
            _sensorAlertsService.ChangeState(RunningState.START);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
