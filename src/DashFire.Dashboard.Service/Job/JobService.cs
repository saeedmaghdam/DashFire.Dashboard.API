using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DashFire.Dashboard.Domain;
using DashFire.Dashboard.Framework.Cache;
using DashFire.Dashboard.Framework.Constants;
using DashFire.Dashboard.Framework.Services.Job;

namespace DashFire.Dashboard.Service.Job
{
    public class JobService : IJobService
    {
        private readonly AppDbContext _db;
        private readonly DashFireCacheManager _cacheManager;

        public JobService(AppDbContext db, DashFireCacheManager cacheManager)
        {
            _db = db;
            _cacheManager = cacheManager;
        }

        public Task<IEnumerable<IJob>> GetAsync(CancellationToken cancellationToken)
        {
            var jobs = _db.Jobs.Where(x => x.RecordStatus != RecordStatus.Deleted);
            if (!jobs.Any())
                return Task.FromResult(new List<IJob>().AsEnumerable());

            return Task.FromResult(ToModel(jobs));
        }

        public async Task<IEnumerable<ICachedJob>> GetCachedAsync(CancellationToken cancellationToken)
        {
            var jobs = await _cacheManager.GetJobsAsync(cancellationToken);
            if (!jobs.Any())
                return new List<ICachedJob>().AsEnumerable();

            var result = new List<Models.CachedJobModel>();
            foreach(var job in jobs)
            {
                var jobDetails = await _cacheManager.GetJobDetailsAsync(job.Key, job.InstanceId, cancellationToken);

                result.Add(new Models.CachedJobModel()
                {
                    Status = jobDetails.Status,
                    LastStatusMessage = jobDetails.LastStatusMessage,
                    InstanceId = job.InstanceId,
                    IsOnline = jobDetails.IsOnline,
                    Key = job.Key,
                    LastExecutionDateTime = jobDetails.LastExecutionDateTime,
                    NextExecutionDateTime = jobDetails.NextExecutionDateTime,
                    SystemName = job.SystemName,
                    Description = job.Description,
                    DisplayName = job.DisplayName,
                    RegistrationRequired = job.RegistrationRequired,
                    Parameters = job.Parameters.Select(x=> new Models.JobParameterModel()
                    {
                        Description = x.Description,
                        DisplayName = x.DisplayName,
                        ParameterName = x.ParameterName,
                        TypeFullName = x.TypeFullName
                    }),
                    HeartBitDateTime = new DateTime(jobDetails.HeartBitDateTimeTicks)
                });
            }

            return result;
        }

        public async Task<long> UpsertAsync(string key, string instanceId, string parameters, string systemName, string displayName, string description, bool registrationRequired, CancellationToken cancellationToken)
        {
            if (key == null)
                throw new Exception("Job's key is required.");
            if (instanceId == null)
                throw new Exception("Job's instance id is required.");

            var currentJob = _db.Jobs.Where(x => x.Key == key && x.InstanceId == instanceId && x.RecordStatus != RecordStatus.Deleted).SingleOrDefault();
            if (currentJob == null)
            {
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
                    RegistrationRequired = registrationRequired
                };

                _db.Jobs.Add(job);

                await _db.SaveChangesAsync(cancellationToken);

                return job.Id;
            }

            await _cacheManager.SetJobAsync(key, instanceId, systemName, displayName, description, registrationRequired, parameters, cancellationToken);
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

            await _cacheManager.SetJobAsync(key, instanceId, currentJob.SystemName, currentJob.DisplayName, currentJob.Description, currentJob.RegistrationRequired, currentJob.Parameters, cancellationToken);
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
                HeartBitDateTime = x.HeartBitDateTime
            });
        }
    }
}
