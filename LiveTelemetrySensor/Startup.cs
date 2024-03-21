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
using System.Diagnostics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using LiveTelemetrySensor.SensorAlerts.Models;
using System.Text;
using System;
using Microsoft.AspNetCore.Authorization;
using LiveTelemetrySensor.Common.Middlewares;

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
            services.AddTokenAuthentication(Configuration);
            services.AddSingleton<CommunicationService>();
            services.AddSingleton<TeleProcessorService>();
            services.AddSingleton<LiveSensorFactory>();
            services.AddSingleton<KafkaConsumerService>();
            services.AddSingleton<SensorAlertsService>();
            services.AddSingleton<AdditionalParser>();
            services.AddSingleton<RequestsService>();
            services.AddSingleton<SensorValidator>();
            services.AddSingleton<IConnectionMultiplexer>(provider =>
            {
                ConfigurationOptions options = new ConfigurationOptions()
                {
                    ConnectTimeout = 1000,
                    AllowAdmin = true,
                    AbortOnConnectFail = false,
                    SyncTimeout = 2147483647,
                };
                return ConnectionMultiplexer.Connect(Configuration["Redis:Configuration:ServerAdress"] + "," + options);
            }
            );
            services.AddSingleton<RedisCacheService>();
            services.AddSingleton<RedisCacheHandler>();
            services.AddSingleton<MongoAlertsService>();
            services.AddSingleton<SensorsContainer>();
            services.AddSingleton<SensorsStateHandler>();
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


            //services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            //    .AddJwtBearer(options =>
            //    {
            //        string issuer = Configuration[Constants.JWT_ISSUER_PATH], key = Configuration[Constants.JWT_KEY_PATH];
            //        Debug.WriteLine(issuer + " " + key + " BDSABJKDBAJSKBDJASJK");
            //        options.TokenValidationParameters = new TokenValidationParameters
            //        {
            //            ValidateIssuer = false,
            //            ValidateLifetime = true,
            //            ValidateIssuerSigningKey = false,
            //            ValidateAudience = false,
            //            //ValidIssuer = issuer,
            //            //ValidAudience = issuer,
            //            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
            //        };
            //    });
            //services.AddAuthorization(options =>
            //{
            //    options.DefaultPolicy = new AuthorizationPolicyBuilder()
            //        .RequireAuthenticatedUser()
            //        .Build();
            //});
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
           
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
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
