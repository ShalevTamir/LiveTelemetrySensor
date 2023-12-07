using LiveTelemetrySensor.SensorAlerts.Services;
using LiveTelemetrySensor.SensorAlerts.Services.Network;
using LiveTelemetrySensor.Consumer.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using LiveTelemetrySensor.Redis.Interfaces;
using LiveTelemetrySensor.Redis.Services;

namespace LiveTelemetrySensor
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<CommunicationService>();
            services.AddSingleton<TeleProcessorService>();
            services.AddSingleton<SensorPropertiesService>();
            services.AddSingleton<KafkaConsumerService>();
            services.AddSingleton<SensorAlertsService>();
            services.AddSingleton<AdditionalParser>();
            services.AddSingleton<RequestsService>();
            services.AddSingleton<IConnectionMultiplexer>(provider =>
                ConnectionMultiplexer.Connect(Configuration["Redis:Configuration:ServerAdress"])
            );
            services.AddSingleton<RedisCacheService>();

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
           
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
