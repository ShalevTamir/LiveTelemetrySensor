using LiveTelemetrySensor.SensorAlerts.Services;
using LiveTelemetrySensor.SensorAlerts.Services.Network;
using LiveTelemetrySensor.Consumer.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using LiveTelemetrySensor.Redis.Services;
using LiveTelemetrySensor.SensorAlerts.Hubs;
using LiveTelemetrySensor.Mongo.Services;
using LiveTelemetrySensor.SensorAlerts.Models.LiveSensor;

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
            services.AddSingleton<LiveSensorFactory>();
            services.AddSingleton<KafkaConsumerService>();
            services.AddSingleton<SensorAlertsService>();
            services.AddSingleton<AdditionalParser>();
            services.AddSingleton<RequestsService>();
            services.AddSingleton<IConnectionMultiplexer>(provider =>
                ConnectionMultiplexer.Connect(Configuration["Redis:Configuration:ServerAdress"] + ",allowAdmin=true")
            );
            services.AddSingleton<RedisCacheService>();
            services.AddSingleton<RedisCacheHandler>();
            services.AddSingleton<MongoAlertsService>();
            services.AddSingleton<SensorsContainer>();
            services.AddHostedService<StartupService>();

            services.AddControllers();

            services.AddCors(options => options.AddPolicy("CorsPolicy", builder =>
            {
                builder
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
                .WithOrigins("http://localhost:4200");
            }));
            services.AddSignalR()
                .AddJsonProtocol(options => {
                    options.PayloadSerializerOptions.PropertyNamingPolicy = null;
                });
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

            app.UseCors("CorsPolicy");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<SensorAlertsHub>("/sensor-alerts-socket");
                endpoints.MapControllers();
            });
        }
    }
}
