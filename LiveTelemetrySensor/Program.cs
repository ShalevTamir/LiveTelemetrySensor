using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace LiveTelemetrySensor
{
    public class Program
    {
        // TODO: add ability to send new sensors through api 
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
            
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    
                });
    }
}
