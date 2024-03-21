using LiveTelemetrySensor.SensorAlerts.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace LiveTelemetrySensor.Common.Middlewares
{
    public static class AuthenticationExtention
    {
        public static IServiceCollection AddTokenAuthentication(this IServiceCollection services, IConfiguration config)
        {
            var key = Encoding.ASCII.GetBytes(config[Constants.JWT_KEY_PATH]);
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = config[Constants.JWT_ISSUER_PATH],
                };                
            });
            return services;
        }
    }
}
