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

        public StartupService(LiveSensorFactory liveSensorFactory, TeleProcessorService teleProcessorService)
        {
            _liveSensorFactory = liveSensorFactory;
            _teleProcessorService = teleProcessorService;

        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            IEnumerable<Task<BaseSensor>> liveSensors = _liveSensorFactory.BuildLiveSensors();
            foreach(var sensorTask in liveSensors)
            {
                _teleProcessorService.AddSensorToUpdate(await sensorTask);
            }
            
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
