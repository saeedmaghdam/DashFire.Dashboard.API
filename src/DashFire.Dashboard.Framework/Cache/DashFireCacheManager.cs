using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DashFire.Dashboard.Framework.Services.Job;
using MessagePack;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace DashFire.Dashboard.Framework.Cache
{
    public class DashFireCacheManager
    {
        private bool _isInitialized = false;
        private object _locker = new object();

        private readonly IDistributedCache _cache;
        private readonly IServiceProvider _serviceProvider;

        public DashFireCacheManager(IDistributedCache cache, IServiceProvider serviceProvider)
        {
            _cache = cache;
            _serviceProvider = serviceProvider;
        }

        public Task InitializeAsync(CancellationToken cancellationToken)
        {
            if (!_isInitialized)
            {
                lock (_locker)
                {
                    if (!_isInitialized)
                    {
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var jobService = scope.ServiceProvider.GetRequiredService<IJobService>();
                            var jobs = jobService.GetAsync(cancellationToken).GetAwaiter().GetResult();
                            SetJobsAsync(jobs, cancellationToken).GetAwaiter().GetResult();
                        }
                        _isInitialized = true;
                    }
                }
            }

            return Task.CompletedTask;
        }

        public async Task<IEnumerable<Models.JobCacheModel>> GetJobsAsync(CancellationToken cancellationToken)
        {
            var cacheResult = await _cache.GetAsync("jobs", cancellationToken);
            if (cacheResult == null)
                return new List<Models.JobCacheModel>();

            var jobs = MessagePackSerializer.Deserialize<IDictionary<string, Models.JobCacheModel>>(cacheResult);

            return jobs.Select(x=>x.Value);
        }

        public async Task SetJobAsync(string key, string instanceId, CancellationToken cancellationToken)
        {
            var cacheResult = await _cache.GetAsync("jobs", cancellationToken);
            var jobs = default(IDictionary<string, Models.JobCacheModel>);
            if (cacheResult != null)
                jobs = MessagePackSerializer.Deserialize<IDictionary<string, Models.JobCacheModel>>(cacheResult);
            else
                jobs = new Dictionary<string, Models.JobCacheModel>();

            if (!jobs.ContainsKey($"{key}_{instanceId}"))
            {
                jobs.Add($"{key}_{instanceId}", new Models.JobCacheModel()
                {
                    Key = key,
                    InstanceId = instanceId
                });

                var serializedJobs = MessagePackSerializer.Serialize<IDictionary<string, Models.JobCacheModel>>(jobs);
                await _cache.SetAsync("jobs", serializedJobs, cancellationToken);
            }
        }

        public async Task SetJobsAsync(IEnumerable<IJob> jobModels, CancellationToken cancellationToken)
        {
            var cacheResult = await _cache.GetAsync("jobs", cancellationToken);
            var jobs = default(IDictionary<string, Models.JobCacheModel>);
            if (cacheResult != null)
                jobs = MessagePackSerializer.Deserialize<IDictionary<string, Models.JobCacheModel>>(cacheResult);
            else
                jobs = new Dictionary<string, Models.JobCacheModel>();

            foreach (var jobModel in jobModels)
            {
                if (!jobs.ContainsKey($"{jobModel.Key}_{jobModel.InstanceId}"))
                {
                    jobs.Add($"{jobModel.Key}_{jobModel.InstanceId}", new Models.JobCacheModel()
                    {
                        Key = jobModel.Key,
                        InstanceId = jobModel.InstanceId
                    });
                }
            }

            var serializedJobs = MessagePackSerializer.Serialize<IDictionary<string, Models.JobCacheModel>>(jobs);
            await _cache.SetAsync("jobs", serializedJobs, cancellationToken);
        }

        public async Task RemoveAllJobsJobs(CancellationToken cancellationToken)
        {
            await _cache.RemoveAsync("jobs", cancellationToken);
        }
    }
}
