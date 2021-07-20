using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DashFire.Dashboard.Domain;
using DashFire.Dashboard.Framework.Constants;
using DashFire.Dashboard.Framework.Services.Job;

namespace DashFire.Dashboard.Service.Job
{
    public class JobService : IJobService
    {
        private readonly AppDbContext _db;

        public JobService(AppDbContext db)
        {
            _db = db;
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
                    RecordStatus = Framework.Constants.RecordStatus.Inserted,
                    InstanceId = instanceId,
                    IsOnline = false,
                    Parameters = parameters,
                    RecordInsertDateTime = System.DateTime.Now,
                    RecordUpdateDateTime = System.DateTime.Now,
                    Key = key,
                    ViewId = System.Guid.NewGuid()
                };

                _db.Jobs.Add(job);

                await _db.SaveChangesAsync(cancellationToken);

                return job.Id;
            }

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
    }
}
