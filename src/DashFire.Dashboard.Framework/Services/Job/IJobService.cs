using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DashFire.Dashboard.Framework.Constants;

namespace DashFire.Dashboard.Framework.Services.Job
{
    public interface IJobService
    {
        Task<IEnumerable<IJob>> GetAsync(CancellationToken cancellationToken);

        Task<IEnumerable<IJob>> GetServiceModeJobsIncludingAllAliveNotServiceModeJobsAsync(CancellationToken cancellationToken);

        Task<IJob> GetByIdAsync(long id, CancellationToken cancellationToken);

        Task<IJob> GetByKeyInstanceIdAsync(string key, string instanceId, CancellationToken cancellationToken);

        Task<IEnumerable<ICachedJob>> GetCachedAsync(CancellationToken cancellationToken);

        Task<long> UpsertAsync(string key, string instanceId, string parameters, string systemName, string displayName, string description, bool registrationRequired, JobExecutionMode jobExecutionMode, string originalInstanceId, CancellationToken cancellationToken);

        Task PatchJobHeartBitAsync(string key, string instanceId, CancellationToken cancellationToken);

        Task PatchJobStatusAsync(string key, string instanceId, JobStatus jobStatus, CancellationToken cancellationToken);

        Task PatchJobStatusMessageAsync(string key, string instanceId, string jobStatusMessage, CancellationToken cancellationToken);

        Task PatchJobNextExecutionDateTimeAsync(string key, string instanceId, DateTime nextExecutionDateTime, CancellationToken cancellationToken);

        Task PatchJobToOfflineAsync(string key, string instanceId, CancellationToken cancellationToken);

        Task PatchJobToShutdownAsync(string key, string instanceId, CancellationToken cancellationToken);

        Task ExecuteAsync(string key, string instanceId, IEnumerable<IJobParameterValue> parameters, CancellationToken cancellationToken);
    }
}
