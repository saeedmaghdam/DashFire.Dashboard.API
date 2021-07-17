using System.Threading;
using System.Threading.Tasks;

namespace DashFire.Dashboard.Framework.Services.Job
{
    public interface IJobService
    {
        Task<long> UpsertAsync(string key, string instanceId, string parameters, CancellationToken cancellationToken);
    }
}
