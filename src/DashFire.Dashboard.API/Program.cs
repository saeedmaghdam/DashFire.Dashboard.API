using DashFire.Dashboard.API.Workers.Subscribers;
using DashFire.Dashboard.API.Workers.Subscribers.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DashFire.Dashboard.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureServices(services =>
                    {
                        services.AddHostedService<RegistrationSubscriber>();
                        services.AddHostedService<HeartBitSubscriber>();
                    });
                    webBuilder.UseStartup<Startup>();
                });
    }
}
