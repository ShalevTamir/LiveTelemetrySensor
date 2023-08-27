using LiveTelemetrySensor.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace LiveTelemetrySensor.Consumer.Services
{
    /*
    public class ConsumerHostedService : IHostedService
    {
        private readonly KafkaConsumerService _consumerService;
        public ConsumerHostedService(KafkaConsumerService kafkaConsumerService)
        {
            _consumerService = kafkaConsumerService;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _consumerService.StartConsumer();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }*/
}
