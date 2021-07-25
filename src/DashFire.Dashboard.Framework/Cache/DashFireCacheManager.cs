using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DashFire.Dashboard.Framework.Constants;
using DashFire.Dashboard.Framework.SerializerOptions;
using DashFire.Dashboard.Framework.Services.Job;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace DashFire.Dashboard.Framework.Cache
{
    public class DashFireCacheManager
    {
        private const string CacheKeyJob = "jobs";
        private const string CacheKeyJobDetails = "job_details";
        private const string CacheKeyJobWithDetails = "jobs_with_details";

        private bool _isInitialized = false;
        private object _locker = new object();

        private readonly IDistributedCache _cache;
        private readonly IServiceProvider _serviceProvider;

        private readonly MessagePackSerializerOptions _options;

        public DashFireCacheManager(IDistributedCache cache, IServiceProvider serviceProvider)
        {
            _cache = cache;
            _serviceProvider = serviceProvider;

            var resolver = CompositeResolver.Create(
                new[] { new DateTimeFormatter() },
                new[] { StandardResolver.Instance });
            _options = MessagePackSerializerOptions.Standard.WithResolver(resolver);
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
                            RemoveAllJobs(cancellationToken).GetAwaiter().GetResult();

                            var jobService = scope.ServiceProvider.GetRequiredService<IJobService>();
                            var jobs = jobService.GetServiceModeJobsIncludingAllAliveNotServiceModeJobsAsync(cancellationToken).GetAwaiter().GetResult();
                            InitializeCacheAsync(jobs, cancellationToken).GetAwaiter().GetResult();
                        }
                        _isInitialized = true;
                    }
                }
            }

            return Task.CompletedTask;
        }

        public async Task<IEnumerable<Models.JobCacheModel>> GetJobsAsync(CancellationToken cancellationToken)
        {
            var cacheResult = await _cache.GetAsync(CacheKeyJob, cancellationToken);
            if (cacheResult == null)
                return new List<Models.JobCacheModel>();

            var jobs = MessagePackSerializer.Deserialize<IDictionary<string, Models.JobCacheModel>>(cacheResult, _options);

            return jobs.Select(x => x.Value);
        }

        public async Task<Models.JobDetailsCacheModel> GetJobDetailsAsync(string key, string instanceId, CancellationToken cancellationToken)
        {
            var cacheResult = await _cache.GetAsync($"{CacheKeyJobDetails}_{key}_{instanceId}", cancellationToken);
            if (cacheResult == null)
                return default(Models.JobDetailsCacheModel);

            return MessagePackSerializer.Deserialize<Models.JobDetailsCacheModel>(cacheResult, _options);
        }

        public async Task<IEnumerable<Models.JobWithDetailsCacheModel>> GetJobsWithDetailsAsync(CancellationToken cancellationToken)
        {
            var cacheResult = await _cache.GetAsync(CacheKeyJobWithDetails, cancellationToken);
            if (cacheResult == null)
            {
                var jobs = await GetJobsAsync(cancellationToken);
                if (!jobs.Any())
                    return new List<Models.JobWithDetailsCacheModel>().AsEnumerable();

                var result = new List<Models.JobWithDetailsCacheModel>();
                foreach (var job in jobs)
                {
                    var jobDetails = await GetJobDetailsAsync(job.Key, job.InstanceId, cancellationToken);

                    result.Add(new Models.JobWithDetailsCacheModel()
                    {
                        Description = job.Description,
                        DisplayName = job.DisplayName,
                        HeartBitDateTimeTicks = jobDetails.HeartBitDateTimeTicks,
                        InstanceId = job.InstanceId,
                        JobExecutionMode = job.JobExecutionMode,
                        IsOnline = jobDetails.IsOnline,
                        Key = job.Key,
                        LastExecutionDateTime = jobDetails.LastExecutionDateTime,
                        LastStatusMessage = jobDetails.LastStatusMessage,
                        NextExecutionDateTime = jobDetails.NextExecutionDateTime,
                        OriginalInstanceId = job.OriginalInstanceId,
                        Parameters = job.Parameters,
                        RegistrationRequired = job.RegistrationRequired,
                        Status = jobDetails.Status,
                        SystemName = job.SystemName
                    });
                }

                var serializedJobsWithDetails = MessagePackSerializer.Serialize<List<Models.JobWithDetailsCacheModel>>(result, _options);
                var options = new DistributedCacheEntryOptions().SetAbsoluteExpiration(DateTime.Now.AddSeconds(3));
                await _cache.SetAsync(CacheKeyJobWithDetails, serializedJobsWithDetails, options, cancellationToken);

                return result;
            }

            var resultItems = MessagePackSerializer.Deserialize<IEnumerable<Models.JobWithDetailsCacheModel>>(cacheResult, _options);

            return resultItems;
        }

        public async Task SetJobAsync(string key, string instanceId, string systemName, string displayName, string description, bool registrationRequired, string parameters, JobExecutionMode jobExecutionMode, string originalInstanceId, CancellationToken cancellationToken)
        {
            var cacheResult = await _cache.GetAsync(CacheKeyJob, cancellationToken);
            var jobs = default(IDictionary<string, Models.JobCacheModel>);
            if (cacheResult != null)
                jobs = MessagePackSerializer.Deserialize<IDictionary<string, Models.JobCacheModel>>(cacheResult, _options);
            else
                jobs = new Dictionary<string, Models.JobCacheModel>();

            if (!jobs.ContainsKey($"{key}_{instanceId}"))
            {
                jobs.Add($"{key}_{instanceId}", new Models.JobCacheModel()
                {
                    Key = key,
                    InstanceId = instanceId,
                    SystemName = systemName,
                    Description = description,
                    DisplayName = displayName,
                    RegistrationRequired = registrationRequired,
                    Parameters = JsonSerializer.Deserialize<List<Models.JobParameterCacheModel>>(parameters),
                    JobExecutionMode = jobExecutionMode,
                    OriginalInstanceId = originalInstanceId
                });

                var serializedJobs = MessagePackSerializer.Serialize<IDictionary<string, Models.JobCacheModel>>(jobs, _options);
                await _cache.SetAsync(CacheKeyJob, serializedJobs, cancellationToken);

                if (await GetJobDetailsAsync(key, instanceId, cancellationToken) == null)
                {
                    var jobDetails = new Models.JobDetailsCacheModel()
                    {
                        Key = key,
                        InstanceId = instanceId,
                        LastStatusMessage = "",
                        Status = JobStatus.New,
                        IsOnline = false,
                        LastExecutionDateTime = 0,
                        NextExecutionDateTime = 0,
                        HeartBitDateTimeTicks = 0
                    };
                    var serializedJobDetails = MessagePackSerializer.Serialize<Models.JobDetailsCacheModel>(jobDetails, _options);
                    await _cache.SetAsync($"{CacheKeyJobDetails}_{key}_{instanceId}", serializedJobDetails, cancellationToken);
                }
            }
        }

        public async Task SetJobStatusMessageAsync(string key, string instanceId, string message, CancellationToken cancellationToken)
        {
            var jobDetails = await GetJobDetailsAsync(key, instanceId, cancellationToken);
            if (jobDetails == null)
                return;

            jobDetails.LastStatusMessage = message;

            var serializedJobDetails = MessagePackSerializer.Serialize<Models.JobDetailsCacheModel>(jobDetails, _options);
            await _cache.SetAsync($"{CacheKeyJobDetails}_{key}_{instanceId}", serializedJobDetails, cancellationToken);
        }

        public async Task SetJobStatusAsync(string key, string instanceId, JobStatus jobStatus, CancellationToken cancellationToken)
        {
            var jobDetails = await GetJobDetailsAsync(key, instanceId, cancellationToken);
            if (jobDetails == null)
                return;

            jobDetails.Status = jobStatus;
            if (jobStatus == JobStatus.Running)
                jobDetails.LastExecutionDateTime = DateTime.Now.Ticks;

            var serializedJobDetails = MessagePackSerializer.Serialize<Models.JobDetailsCacheModel>(jobDetails, _options);
            await _cache.SetAsync($"{CacheKeyJobDetails}_{key}_{instanceId}", serializedJobDetails, cancellationToken);
        }

        public async Task SetJobScheduleAsync(string key, string instanceId, DateTime? nextExecutionDateTime, CancellationToken cancellationToken)
        {
            var jobDetails = await GetJobDetailsAsync(key, instanceId, cancellationToken);
            if (jobDetails == null)
                return;

            jobDetails.NextExecutionDateTime = nextExecutionDateTime.HasValue ? nextExecutionDateTime.Value.Ticks : 0;

            var serializedJobDetails = MessagePackSerializer.Serialize<Models.JobDetailsCacheModel>(jobDetails, _options);
            await _cache.SetAsync($"{CacheKeyJobDetails}_{key}_{instanceId}", serializedJobDetails, cancellationToken);
        }

        public async Task SetJobHeartBitAsync(string key, string instanceId, CancellationToken cancellationToken)
        {
            var jobDetails = await GetJobDetailsAsync(key, instanceId, cancellationToken);
            if (jobDetails == null)
                return;

            jobDetails.HeartBitDateTimeTicks = DateTime.Now.Ticks;

            var serializedJobDetails = MessagePackSerializer.Serialize<Models.JobDetailsCacheModel>(jobDetails, _options);
            await _cache.SetAsync($"{CacheKeyJobDetails}_{key}_{instanceId}", serializedJobDetails, cancellationToken);
        }

        public async Task SetJobOfflineAsync(string key, string instanceId, CancellationToken cancellationToken)
        {
            var jobDetails = await GetJobDetailsAsync(key, instanceId, cancellationToken);
            if (jobDetails == null)
                return;

            jobDetails.IsOnline = false;

            var serializedJobDetails = MessagePackSerializer.Serialize<Models.JobDetailsCacheModel>(jobDetails, _options);
            await _cache.SetAsync($"{CacheKeyJobDetails}_{key}_{instanceId}", serializedJobDetails, cancellationToken);
        }

        public async Task SetJobOnlineAsync(string key, string instanceId, CancellationToken cancellationToken)
        {
            var jobDetails = await GetJobDetailsAsync(key, instanceId, cancellationToken);
            if (jobDetails == null)
                return;

            jobDetails.IsOnline = true;

            var serializedJobDetails = MessagePackSerializer.Serialize<Models.JobDetailsCacheModel>(jobDetails, _options);
            await _cache.SetAsync($"{CacheKeyJobDetails}_{key}_{instanceId}", serializedJobDetails, cancellationToken);
        }

        public async Task RemoveAllJobs(CancellationToken cancellationToken)
        {
            var jobs = GetJobsAsync(cancellationToken).GetAwaiter().GetResult();

            await _cache.RemoveAsync(CacheKeyJob, cancellationToken);

            foreach (var job in jobs)
            {
                await _cache.RemoveAsync($"{CacheKeyJobDetails}_{job.Key}_{job.InstanceId}", cancellationToken);
            }
        }

        public async Task RemoveJobAsync(string key, string instanceId, CancellationToken cancellationToken)
        {
            var cacheResult = await _cache.GetAsync(CacheKeyJob, cancellationToken);
            if (cacheResult == null)
                return;

            var jobs = MessagePackSerializer.Deserialize<IDictionary<string, Models.JobCacheModel>>(cacheResult, _options);
            if (jobs.ContainsKey($"{key}_{instanceId}"))
            {
                jobs.Remove($"{key}_{instanceId}");

                var serializedJobs = MessagePackSerializer.Serialize<IDictionary<string, Models.JobCacheModel>>(jobs, _options);
                await _cache.SetAsync(CacheKeyJob, serializedJobs, cancellationToken);
            }

            await _cache.RemoveAsync($"{CacheKeyJobDetails}_{key}_{instanceId}", cancellationToken);
        }

        private async Task InitializeCacheAsync(IEnumerable<IJob> jobModels, CancellationToken cancellationToken)
        {
            var jobs = new Dictionary<string, Models.JobCacheModel>();

            foreach (var jobModel in jobModels)
            {
                jobs.Add($"{jobModel.Key}_{jobModel.InstanceId}", new Models.JobCacheModel()
                {
                    Key = jobModel.Key,
                    InstanceId = jobModel.InstanceId,
                    SystemName = jobModel.SystemName,
                    Description = jobModel.Description,
                    DisplayName = jobModel.DisplayName,
                    RegistrationRequired = jobModel.RegistrationRequired,
                    Parameters = JsonSerializer.Deserialize<List<Models.JobParameterCacheModel>>(jobModel.Parameters),
                    JobExecutionMode = jobModel.JobExecutionMode,
                    OriginalInstanceId = jobModel.OriginalJob?.InstanceId
                });


                var jobDetails = new Models.JobDetailsCacheModel()
                {
                    Key = jobModel.Key,
                    InstanceId = jobModel.InstanceId,
                    LastStatusMessage = jobModel.LastStatusMessage,
                    Status = jobModel.Status,
                    IsOnline = jobModel.IsOnline,
                    LastExecutionDateTime = jobModel.LastExecutionDateTime.HasValue ? jobModel.LastExecutionDateTime.Value.Ticks : 0,
                    NextExecutionDateTime = jobModel.NextExecutionDateTime.HasValue ? jobModel.NextExecutionDateTime.Value.Ticks : 0,
                    HeartBitDateTimeTicks = jobModel.HeartBitDateTime.HasValue ? jobModel.HeartBitDateTime.Value.Ticks : 0
                };
                var serializedJobDetails = MessagePackSerializer.Serialize<Models.JobDetailsCacheModel>(jobDetails, _options);
                await _cache.SetAsync($"{CacheKeyJobDetails}_{jobModel.Key}_{jobModel.InstanceId}", serializedJobDetails, cancellationToken);
            }

            var serializedJobs = MessagePackSerializer.Serialize<IDictionary<string, Models.JobCacheModel>>(jobs, _options);
            await _cache.SetAsync(CacheKeyJob, serializedJobs, cancellationToken);
        }
    }
}
