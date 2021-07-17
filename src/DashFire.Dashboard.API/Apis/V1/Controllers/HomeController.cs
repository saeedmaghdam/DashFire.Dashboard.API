using System.Threading;
using System.Threading.Tasks;
using DashFire.Dashboard.Framework.Services.Job;
using Microsoft.AspNetCore.Mvc;

namespace DashFire.Dashboard.API.Apis.V1.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class HomeController : Controller
    {
        public HomeController()
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            return Ok("Done!");
        }
    }
}
