using System.IO;
using System.Reflection;
using DashFire.Dashboard.API.Workers.Jobs;
using DashFire.Dashboard.API.Workers.Subscribers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
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
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
                    config.AddJsonFile("appsettings.json", optional: true);
                    config.AddEnvironmentVariables();
                })
                .ConfigureServices(services =>
                {
                    services.AddHostedService<JobExpirationValidatorJob>();

                    services.AddHostedService<RegistrationSubscriber>();
                    services.AddHostedService<HeartBitSubscriber>();
                    services.AddHostedService<JobStatusSubscriber>();
                    services.AddHostedService<LogJobStatusSubscriber>();
                    services.AddHostedService<JobScheduleSubscriber>();
                    services.AddHostedService<ShutdownSubscriber>();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
