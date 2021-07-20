﻿using System.Threading;
using System.Threading.Tasks;
using DashFire.Dashboard.Framework.Constants;

namespace DashFire.Dashboard.Framework.Services.Job
{
    public interface IJobService
    {
        Task<long> UpsertAsync(string key, string instanceId, string parameters, CancellationToken cancellationToken);

        Task PatchJobExecutionStatusAsync(string key, string instanceId, CancellationToken cancellationToken);

        Task PatchJobStatusAsync(string key, string instanceId, JobStatus jobStatus, CancellationToken cancellationToken);

        Task PatchJobStatusMessageAsync(string key, string instanceId, string jobStatusMessage, CancellationToken cancellationToken);
    }
}
