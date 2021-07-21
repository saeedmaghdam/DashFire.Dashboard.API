using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DashFire.Dashboard.Framework.Services.Job;
using Microsoft.AspNetCore.Mvc;

namespace DashFire.Dashboard.API.Apis.V1.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class JobController : Controller
    {
        private readonly IJobService _jobService;

        public JobController(IJobService jobService)
        {
            _jobService = jobService;
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
                })
            }));
        }
    }
}
