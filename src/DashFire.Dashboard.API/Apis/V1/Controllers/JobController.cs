using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DashFire.Dashboard.Framework.Options;
using DashFire.Dashboard.Framework.Services.Job;
using DashFire.Dashboard.Framework.Services.Log;
using DashFire.Dashboard.Service.Job.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DashFire.Dashboard.API.Apis.V1.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class JobController : Controller
    {
        private readonly IJobService _jobService;
        private readonly ILogService _logService;

        public JobController(IOptions<ApplicationOptions> options, IJobService jobService, ILogService logService)
        {
            _jobService = jobService;
            _logService = logService;
        }

        [HttpGet]
        public async Task<ActionResult<Models.Job.IndexViewModel>> Index(CancellationToken cancellationToken)
        {
            var jobs = await _jobService.GetCachedAsync(cancellationToken);
            if (!jobs.Any())
                return NoContent();

            return Ok(jobs.Select(job => new Models.Job.IndexViewModel()
            {
                Key = job.Key,
                InstanceId = job.InstanceId,
                Status = job.Status,
                IsOnline = job.IsOnline,
                LastStatusMessage = job.LastStatusMessage,
                LastExecutionDateTime = job.LastExecutionDateTime,
                NextExecutionDateTime = job.NextExecutionDateTime,
                SystemName = job.SystemName,
                Description = job.Description,
                DisplayName = job.DisplayName,
                RegistrationRequired = job.RegistrationRequired,
                Parameters = job.Parameters.Select(x=> new Models.Job.JobParameterViewModel()
                {
                    Description = x.Description,
                    DisplayName = x.DisplayName,
                    ParameterName = x.ParameterName,
                    TypeFullName = x.TypeFullName
                }),
                HeartBitDateTime = job.HeartBitDateTime,
                JobExecutionMode = job.JobExecutionMode,
                OriginalInstanceId = job.OriginalInstanceId
            }));
        }

        [HttpGet("{id}/Logs")]
        public async Task<ActionResult<List<Models.Job.LogViewModel>>> GetLogsAsync([FromRoute] long id, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] int? totalItems, CancellationToken cancellationToken)
        {
            var logs = await _logService.GetAsync(id, startDate, endDate, totalItems, cancellationToken);
            if (!logs.Any())
                return NoContent();

            return Ok(logs.Select(log => new Models.Job.LogViewModel()
            {
                Message = log.Message,
                RecordInsertDateTime = log.RecordInsertDateTime
            }));
        }

        [HttpPost("{key}/{instanceId}/Execute")]
        public async Task<ActionResult> ExecuteAsync([FromRoute] string key, [FromRoute] string instanceId, [FromBody] Models.Job.ExecuteInputModel inputModel, CancellationToken cancellationToken)
        {
            var job = await _jobService.GetByKeyInstanceIdAsync(key, instanceId, cancellationToken);
            if (job == null)
                return NotFound();

            await _jobService.ExecuteAsync(key, instanceId, inputModel.Parameters.Select(x => new JobParameterValueModel()
            {
                ParameterName = x.ParameterName,
                Value = x.Value
            }), cancellationToken);

            return Ok();
        }
    }
}
