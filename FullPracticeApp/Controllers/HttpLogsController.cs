using FullPracticeApp.Contracts.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FullPracticeApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HttpLogsController : Controller
    {
        private readonly IHttpLogsService httpLogsService;
        public HttpLogsController(IHttpLogsService httpLogsService)
        {
            this.httpLogsService = httpLogsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllLogs()
        {
            var logs = await httpLogsService.GetAllLogs();
            return Ok(logs);
        }
    }
}
