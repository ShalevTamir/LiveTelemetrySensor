using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;

namespace LiveTelemetrySensor.Consumer.Services
{
    public class KafkaConsumerService
    {
        private ConsumerConfig _consumerConfig;
        private CancellationTokenSource _currentTokenSource;
        private readonly IConfiguration _configuration;
        

        public KafkaConsumerService(IConfiguration configuration)
        {
            _configuration = configuration;
            _consumerConfig = configuration.GetSection("Consumer:Configuration").Get<ConsumerConfig>();
            _consumerConfig.GroupId = Guid.NewGuid().ToString();
            _consumerConfig.AutoOffsetReset = AutoOffsetReset.Latest;
        }

        public void StartConsumer(Func<string,Task> processDataCallback)
        {
            SetupToken();
            Task.Factory.StartNew(()=>StartConsumerLogic(processDataCallback),_currentTokenSource.Token);
        }

        public void StopConsumer()
        {
            _currentTokenSource.Cancel();
        }
        private IConsumer<Null,string> OpenConsumer()
        {
            return new ConsumerBuilder<Null, string>(_consumerConfig).Build(); 
        }
        private void SetupToken()
        {
            _currentTokenSource = new CancellationTokenSource();           
        }
        private async Task StartConsumerLogic(Func<string, Task> processDataCallback)
        {
            
            using (var consumer = OpenConsumer())
            {
                try
                {
                    consumer.Subscribe(_configuration["Consumer:TopicName"]);

                    while (!_currentTokenSource.IsCancellationRequested)
                    {
                        ConsumeResult<Null, string> consumerResult = consumer.Consume(_currentTokenSource.Token);
                        await processDataCallback(consumerResult.Message.Value);
                    }
                }
                catch(OperationCanceledException)
                {
                    consumer.Close();                       
                }
            }
        }

        

    }
}
