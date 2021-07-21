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

        public async Task<long> UpsertAsync(string key, string instanceId, string parameters, CancellationToken cancellationToken)
        {
            if (key == null)
                throw new Exception("Job's key is required.");
            if (instanceId == null)
                throw new Exception("Job's instance id is required.");

            var currentJob = _db.Jobs.Where(x => x.Key == key && x.InstanceId == instanceId).SingleOrDefault();
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
                    ViewId = Guid.NewGuid()
                };

                _db.Jobs.Add(job);

                await _db.SaveChangesAsync(cancellationToken);

                return job.Id;
            }

            await _cacheManager.SetJobAsync(key, instanceId, cancellationToken);

            return currentJob.Id;
        }

        public async Task PatchJobExecutionStatusAsync(string key, string instanceId, CancellationToken cancellationToken)
        {
            if (key == null)
                throw new Exception("Job's key is required.");
            if (instanceId == null)
                throw new Exception("Job's instance id is required.");

            var currentJob = _db.Jobs.Where(x => x.Key == key && x.InstanceId == instanceId).SingleOrDefault();
            if (currentJob == null)
                throw new Exception($"Job with key {key} and instance id {instanceId} not found.");

            var now = DateTime.Now;
            currentJob.LastExecutionDateTime = now;
            currentJob.RecordUpdateDateTime = now;
            currentJob.IsOnline = true;

            await _db.SaveChangesAsync(cancellationToken);

            await _cacheManager.SetJobAsync(key, instanceId, cancellationToken);
        }

        public async Task PatchJobStatusAsync(string key, string instanceId, JobStatus jobStatus, CancellationToken cancellationToken)
        {
            if (key == null)
                throw new Exception("Job's key is required.");
            if (instanceId == null)
                throw new Exception("Job's instance id is required.");

            var currentJob = _db.Jobs.Where(x => x.Key == key && x.InstanceId == instanceId).SingleOrDefault();
            if (currentJob == null)
                throw new Exception($"Job with key {key} and instance id {instanceId} not found.");

            var now = DateTime.Now;
            currentJob.LastExecutionDateTime = now;
            currentJob.RecordUpdateDateTime = now;
            currentJob.IsOnline = true;
            currentJob.Status = (short)jobStatus;

            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task PatchJobStatusMessageAsync(string key, string instanceId, string jobStatusMessage, CancellationToken cancellationToken)
        {
            if (key == null)
                throw new Exception("Job's key is required.");
            if (instanceId == null)
                throw new Exception("Job's instance id is required.");

            var currentJob = _db.Jobs.Where(x => x.Key == key && x.InstanceId == instanceId).SingleOrDefault();
            if (currentJob == null)
                throw new Exception($"Job with key {key} and instance id {instanceId} not found.");

            var now = DateTime.Now;
            currentJob.LastExecutionDateTime = now;
            currentJob.RecordUpdateDateTime = now;
            currentJob.IsOnline = true;
            currentJob.LastStatusMessage = jobStatusMessage;

            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task PatchJobNextExecutionDateTimeAsync(string key, string instanceId, DateTime nextExecutionDateTime, CancellationToken cancellationToken)
        {
            if (key == null)
                throw new Exception("Job's key is required.");
            if (instanceId == null)
                throw new Exception("Job's instance id is required.");

            var currentJob = _db.Jobs.Where(x => x.Key == key && x.InstanceId == instanceId).SingleOrDefault();
            if (currentJob == null)
                throw new Exception($"Job with key {key} and instance id {instanceId} not found.");

            var now = DateTime.Now;
            currentJob.LastExecutionDateTime = now;
            currentJob.RecordUpdateDateTime = now;
            currentJob.IsOnline = true;
            currentJob.NextExecutionDateTime = nextExecutionDateTime;

            await _db.SaveChangesAsync(cancellationToken);
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
                ViewId = x.ViewId
            });
        }
    }
}
