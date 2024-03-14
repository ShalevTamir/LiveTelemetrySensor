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

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Task<IEnumerable<BaseSensor>> liveSensors = _liveSensorFactory.BuildDefaultLiveSensorsAsync();
            _teleProcessorService.AddSensorsToUpdate(await liveSensors);
            _sensorAlertsService.ChangeState(RunningState.START);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
