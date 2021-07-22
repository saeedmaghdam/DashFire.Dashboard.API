using System;
using System.Threading;
using System.Threading.Tasks;
using DashFire.Dashboard.Framework.Services.Job;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DashFire.Dashboard.API.Workers.Jobs
{
    public class JobExpirationValidatorJob : IHostedService
    {
        private readonly ILogger<JobExpirationValidatorJob> _logger;
        private readonly IServiceProvider _serviceProvider;

        public JobExpirationValidatorJob(ILogger<JobExpirationValidatorJob> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(async () => await Process(cancellationToken));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async Task Process(CancellationToken cancellationToken)
        {
            do
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var jobService = scope.ServiceProvider.GetRequiredService<IJobService>();

                    var jobs = await jobService.GetCachedAsync(cancellationToken);
                    var now = DateTime.Now;
                    foreach (var job in jobs)
                    {
                        if (!job.HeartBitDateTime.HasValue)
                        {
                            await jobService.PatchJobToOfflineAsync(job.Key, job.InstanceId, cancellationToken);

                            continue;
                        }

                        if ((now - job.HeartBitDateTime.Value).TotalSeconds > 60)
                            await jobService.PatchJobToOfflineAsync(job.Key, job.InstanceId, cancellationToken);
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            } while (!cancellationToken.IsCancellationRequested);
        }
    }
}
