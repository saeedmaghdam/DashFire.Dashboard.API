using System.Threading;
using System.Threading.Tasks;
using DashFire.Dashboard.Framework.Constants;

namespace DashFire.Dashboard.Framework.Services.Job
{
    public interface IJobService
    {
        Task<long> UpsertAsync(string key, string instanceId, string parameters, CancellationToken cancellationToken);

        Task PatchJobExecutionStatus(string key, string instanceId, CancellationToken cancellationToken);

        Task PatchJobStatus(string key, string instanceId, JobStatus jobStatus, CancellationToken cancellationToken);
    }
}
