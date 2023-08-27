using LiveTelemetrySensor.SensorAlerts.Models.Enums;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace LiveTelemetrySensor.SensorAlerts.Services
{
    public class SensorAlertsStarter : IHostedService
    {
        SensorAlertsService _sensorAlertsService;
        public SensorAlertsStarter(SensorAlertsService sensorAlertsService)
        {
            _sensorAlertsService = sensorAlertsService;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _sensorAlertsService.ChangeState(RunningState.START);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            //throw new System.NotImplementedException();
        }
    }
}
