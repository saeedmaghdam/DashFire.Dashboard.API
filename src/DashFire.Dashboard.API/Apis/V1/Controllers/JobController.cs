using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DashFire.Dashboard.Framework.Cache;
using Microsoft.AspNetCore.Mvc;

namespace DashFire.Dashboard.API.Apis.V1.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class JobController : Controller
    {
        private readonly DashFireCacheManager _cacheManager;

        public JobController(DashFireCacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var jobs = await _cacheManager.GetJobsAsync(cancellationToken);
            return Ok(jobs.Select(x=> new Models.Job.IndexViewModel()
            {
                Key = x.Key,
                InstanceId = x.InstanceId
            }));
        }
    }
}
