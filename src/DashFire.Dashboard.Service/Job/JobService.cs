using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DashFire.Dashboard.Domain;
using DashFire.Dashboard.Framework;
using DashFire.Dashboard.Framework.Cache;
using DashFire.Dashboard.Framework.Constants;
using DashFire.Dashboard.Framework.Options;
using DashFire.Dashboard.Framework.Services.Job;
using DashFire.Dashboard.Service.Job.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace DashFire.Dashboard.Service.Job
{
    public class JobService : IJobService
    {
        private const string _dashboardSideExchangeName = "DashFire.Service";

        private readonly AppDbContext _db;
        private readonly DashFireCacheManager _cacheManager;
        private readonly IOptions<ApplicationOptions> _options;

        private readonly IConnection _connection;
        private readonly IModel _channel;

        public JobService(AppDbContext db, DashFireCacheManager cacheManager, IOptions<ApplicationOptions> options)
        {
            _db = db;
            _cacheManager = cacheManager;
            _options = options;

            var factory = new ConnectionFactory() { Uri = new Uri(_options.Value.RabbitMqOptions.ConnectionString) };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public Task<IEnumerable<IJob>> GetAsync(CancellationToken cancellationToken)
        {
            var jobs = _db.Jobs.Include(x => x.OriginalJob).Where(x => x.RecordStatus != RecordStatus.Deleted);
            if (!jobs.Any())
                return Task.FromResult(new List<IJob>().AsEnumerable());

            return Task.FromResult(ToModel(jobs));
        }

        public Task<IEnumerable<IJob>> GetServiceModeJobsIncludingAllAliveNotServiceModeJobsAsync(CancellationToken cancellationToken)
        {
            var jobs = _db.Jobs.Include(x => x.OriginalJob).Where(x => (x.OriginalJob == null || x.OriginalJob != null && x.Status != (short)JobStatus.Shutdown) && x.RecordStatus != RecordStatus.Deleted);

            if (!jobs.Any())
                return Task.FromResult(new List<IJob>().AsEnumerable());

            return Task.FromResult(ToModel(jobs));
        }

        public async Task<IJob> GetByIdAsync(long id, CancellationToken cancellationToken)
        {
            var job = await _db.Jobs.Include(x => x.OriginalJob).SingleOrDefaultAsync(x => x.Id == id && x.RecordStatus != RecordStatus.Deleted);
            if (job == null)
                return null;

            return ToModel(new[] { job }).Single();
        }

        public async Task<IJob> GetByKeyInstanceIdAsync(string key, string instanceId, CancellationToken cancellationToken)
        {
            var job = await _db.Jobs.Include(x => x.OriginalJob).SingleOrDefaultAsync(x => x.Key == key && x.InstanceId == instanceId && x.RecordStatus != RecordStatus.Deleted);
            if (job == null)
                return null;

            return ToModel(new[] { job }).Single();
        }

        public async Task<IEnumerable<ICachedJob>> GetCachedAsync(CancellationToken cancellationToken)
        {
            var jobs = await _cacheManager.GetJobsAsync(cancellationToken);
            if (!jobs.Any())
                return new List<ICachedJob>().AsEnumerable();

            var result = new List<Models.CachedJobModel>();
            foreach (var job in jobs)
            {
                var jobDetails = await _cacheManager.GetJobDetailsAsync(job.Key, job.InstanceId, cancellationToken);

                result.Add(new Models.CachedJobModel()
                {
                    Status = jobDetails.Status,
                    LastStatusMessage = jobDetails.LastStatusMessage,
                    InstanceId = job.InstanceId,
                    IsOnline = jobDetails.IsOnline,
                    Key = job.Key,
                    LastExecutionDateTime = jobDetails.LastExecutionDateTime == 0 ? null : new DateTime(jobDetails.LastExecutionDateTime),
                    NextExecutionDateTime = jobDetails.NextExecutionDateTime == 0 ? null : new DateTime(jobDetails.NextExecutionDateTime),
                    SystemName = job.SystemName,
                    Description = job.Description,
                    DisplayName = job.DisplayName,
                    RegistrationRequired = job.RegistrationRequired,
                    Parameters = job.Parameters.Select(x => new Models.JobParameterModel()
                    {
                        Description = x.Description,
                        DisplayName = x.DisplayName,
                        ParameterName = x.ParameterName,
                        TypeFullName = x.TypeFullName
                    }),
                    HeartBitDateTime = new DateTime(jobDetails.HeartBitDateTimeTicks),
                    JobExecutionMode = job.JobExecutionMode,
                    OriginalInstanceId = job.OriginalInstanceId
                });
            }

            return result;
        }

        public async Task<long> UpsertAsync(string key, string instanceId, string parameters, string systemName, string displayName, string description, bool registrationRequired, JobExecutionMode jobExecutionMode, string originalInstanceId, CancellationToken cancellationToken)
        {
            if (key == null)
                throw new Exception("Job's key is required.");
            if (instanceId == null)
                throw new Exception("Job's instance id is required.");

            var currentJob = _db.Jobs.Where(x => x.Key == key && x.InstanceId == instanceId && x.RecordStatus != RecordStatus.Deleted).SingleOrDefault();
            if (currentJob == null)
            {
                var originalJob = default(Domain.Job);
                if (!string.IsNullOrEmpty(originalInstanceId))
                    originalJob = _db.Jobs.Where(x => x.Key == key && x.InstanceId == originalInstanceId && x.RecordStatus != RecordStatus.Deleted).Single();

                var job = new Domain.Job()
                {
                    RecordStatus = RecordStatus.Inserted,
                    InstanceId = instanceId,
                    IsOnline = false,
                    Parameters = parameters,
                    RecordInsertDateTime = DateTime.Now,
                    RecordUpdateDateTime = DateTime.Now,
                    Key = key,
                    ViewId = Guid.NewGuid(),
                    SystemName = systemName,
                    DisplayName = displayName,
                    Description = description,
                    RegistrationRequired = registrationRequired,
                    JobExecutionMode = jobExecutionMode,
                    OriginalJobId = originalJob?.Id
                };

                _db.Jobs.Add(job);

                await _db.SaveChangesAsync(cancellationToken);

                currentJob = job;
            }

            await _cacheManager.SetJobAsync(key, instanceId, systemName, displayName, description, registrationRequired, parameters, jobExecutionMode, originalInstanceId, cancellationToken);
            await _cacheManager.SetJobOnlineAsync(key, instanceId, cancellationToken);

            return currentJob.Id;
        }

        public async Task PatchJobHeartBitAsync(string key, string instanceId, CancellationToken cancellationToken)
        {
            if (key == null)
                throw new Exception("Job's key is required.");
            if (instanceId == null)
                throw new Exception("Job's instance id is required.");

            var currentJob = _db.Jobs.Where(x => x.Key == key && x.InstanceId == instanceId && x.RecordStatus != RecordStatus.Deleted).SingleOrDefault();
            if (currentJob == null)
                throw new Exception($"Job with key {key} and instance id {instanceId} not found.");

            var now = DateTime.Now;
            currentJob.RecordStatus = RecordStatus.Updated;
            currentJob.RecordUpdateDateTime = now;
            currentJob.HeartBitDateTime = now;
            currentJob.IsOnline = true;

            await _db.SaveChangesAsync(cancellationToken);

            //await _cacheManager.SetJobAsync(key, instanceId, currentJob.SystemName, currentJob.DisplayName, currentJob.Description, currentJob.RegistrationRequired, currentJob.Parameters, currentJob.JobExecutionMode, currentJob.OriginalJob?.InstanceId, cancellationToken);
            await _cacheManager.SetJobHeartBitAsync(key, instanceId, cancellationToken);
            await _cacheManager.SetJobOnlineAsync(key, instanceId, cancellationToken);
        }

        public async Task PatchJobStatusAsync(string key, string instanceId, JobStatus jobStatus, CancellationToken cancellationToken)
        {
            if (key == null)
                throw new Exception("Job's key is required.");
            if (instanceId == null)
                throw new Exception("Job's instance id is required.");

            var currentJob = _db.Jobs.Where(x => x.Key == key && x.InstanceId == instanceId && x.RecordStatus != RecordStatus.Deleted).SingleOrDefault();
            if (currentJob == null)
                throw new Exception($"Job with key {key} and instance id {instanceId} not found.");

            var now = DateTime.Now;
            if (jobStatus == JobStatus.Running)
                currentJob.LastExecutionDateTime = now;
            currentJob.RecordStatus = RecordStatus.Updated;
            currentJob.RecordUpdateDateTime = now;
            currentJob.IsOnline = true;
            currentJob.Status = (short)jobStatus;

            await _db.SaveChangesAsync(cancellationToken);

            await _cacheManager.SetJobStatusAsync(key, instanceId, jobStatus, cancellationToken);
        }

        public async Task PatchJobStatusMessageAsync(string key, string instanceId, string jobStatusMessage, CancellationToken cancellationToken)
        {
            if (key == null)
                throw new Exception("Job's key is required.");
            if (instanceId == null)
                throw new Exception("Job's instance id is required.");

            var currentJob = _db.Jobs.Where(x => x.Key == key && x.InstanceId == instanceId && x.RecordStatus != RecordStatus.Deleted).SingleOrDefault();
            if (currentJob == null)
                throw new Exception($"Job with key {key} and instance id {instanceId} not found.");

            var now = DateTime.Now;
            currentJob.RecordStatus = RecordStatus.Updated;
            currentJob.RecordUpdateDateTime = now;
            currentJob.IsOnline = true;
            currentJob.LastStatusMessage = jobStatusMessage;

            await _db.SaveChangesAsync(cancellationToken);

            await _cacheManager.SetJobStatusMessageAsync(key, instanceId, jobStatusMessage, cancellationToken);
        }

        public async Task PatchJobNextExecutionDateTimeAsync(string key, string instanceId, DateTime nextExecutionDateTime, CancellationToken cancellationToken)
        {
            if (key == null)
                throw new Exception("Job's key is required.");
            if (instanceId == null)
                throw new Exception("Job's instance id is required.");

            var currentJob = _db.Jobs.Where(x => x.Key == key && x.InstanceId == instanceId && x.RecordStatus != RecordStatus.Deleted).SingleOrDefault();
            if (currentJob == null)
                throw new Exception($"Job with key {key} and instance id {instanceId} not found.");

            var now = DateTime.Now;
            currentJob.RecordStatus = RecordStatus.Updated;
            currentJob.RecordUpdateDateTime = now;
            currentJob.IsOnline = true;
            currentJob.NextExecutionDateTime = nextExecutionDateTime;

            await _db.SaveChangesAsync(cancellationToken);

            await _cacheManager.SetJobScheduleAsync(key, instanceId, nextExecutionDateTime, cancellationToken);
        }

        public async Task PatchJobToOfflineAsync(string key, string instanceId, CancellationToken cancellationToken)
        {
            var job = _db.Jobs.Where(x => x.Key == key && x.InstanceId == instanceId && x.RecordStatus != RecordStatus.Deleted).Single();

            job.IsOnline = false;
            job.RecordStatus = RecordStatus.Updated;
            job.RecordUpdateDateTime = DateTime.Now;

            await _db.SaveChangesAsync(cancellationToken);

            await _cacheManager.SetJobOfflineAsync(job.Key, job.InstanceId, cancellationToken);
        }

        public async Task PatchJobToShutdownAsync(string key, string instanceId, CancellationToken cancellationToken)
        {
            var job = _db.Jobs.Where(x => x.Key == key && x.InstanceId == instanceId && x.RecordStatus != RecordStatus.Deleted).Single();

            job.IsOnline = false;
            job.RecordStatus = RecordStatus.Updated;
            job.RecordUpdateDateTime = DateTime.Now;

            await _db.SaveChangesAsync(cancellationToken);

            await _cacheManager.SetJobOfflineAsync(job.Key, job.InstanceId, cancellationToken);
            if (job.OriginalJobId.HasValue)
                await _cacheManager.RemoveJobAsync(key, instanceId, cancellationToken);
        }

        public async Task ExecuteAsync(string key, string instanceId, IEnumerable<IJobParameterValue> parameters, CancellationToken cancellationToken)
        {
            var job = await _db.Jobs.Where(x => x.Key == key && x.InstanceId == instanceId && x.RecordStatus != RecordStatus.Deleted).SingleAsync();

            var headers = new Dictionary<string, object>()
            {
                {
                    "job_key", key
                },
                {
                    "job_instance_id", instanceId
                }
            };

            QueueManager.DeclareExchangeAndQueue(_channel, _dashboardSideExchangeName, key, instanceId, headers);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = false;
            headers.Add("message_type", MessageTypes.JobExecutionRequest.ToString().ToLower());
            properties.Headers = headers;

            var newInstanceId = Guid.NewGuid().ToString();
            var model = new JobExecutionRequestModel()
            {
                Key = key,
                InstanceId = instanceId,
                NewInstanceId = newInstanceId,
                Parameters = parameters
            };
            var messageBodyBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(model));

            await UpsertAsync(key, newInstanceId, job.Parameters, job.SystemName, job.DisplayName, job.Description, true, JobExecutionMode.ServerRequestedMode, job.InstanceId, cancellationToken);
            await PatchJobHeartBitAsync(key, instanceId, cancellationToken);
            await _cacheManager.SetJobAsync(key, newInstanceId, job.SystemName, job.DisplayName, job.Description, true, job.Parameters, JobExecutionMode.ServerRequestedMode, job.InstanceId, cancellationToken);
            _channel.BasicPublish(_dashboardSideExchangeName, "", properties, messageBodyBytes);
        }

        public static IEnumerable<IJob> ToModel(IEnumerable<Domain.Job> jobs)
        {
            return jobs.Select(x => new Models.JobModel()
            {
                Id = x.Id,
                InstanceId = x.InstanceId,
                Status = (JobStatus)x.Status,
                IsOnline = x.IsOnline,
                Key = x.Key,
                LastExecutionDateTime = x.LastExecutionDateTime,
                LastStatusMessage = x.LastStatusMessage,
                NextExecutionDateTime = x.NextExecutionDateTime,
                Parameters = x.Parameters,
                RecordInsertDateTime = x.RecordInsertDateTime,
                RecordStatus = x.RecordStatus,
                RecordUpdateDateTime = x.RecordUpdateDateTime,
                ViewId = x.ViewId,
                SystemName = x.SystemName,
                Description = x.Description,
                DisplayName = x.DisplayName,
                RegistrationRequired = x.RegistrationRequired,
                HeartBitDateTime = x.HeartBitDateTime,
                JobExecutionMode = x.JobExecutionMode,
                OriginalJob = x.OriginalJob == null ? null : ToModel(new[] { x.OriginalJob }).Single()
            });
        }
    }
}
