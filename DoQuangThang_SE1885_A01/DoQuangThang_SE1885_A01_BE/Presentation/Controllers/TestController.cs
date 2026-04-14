using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet("api/test-timeout")]
        public async Task<IActionResult> TestTimeout()
        {
            // Cố tình "ngâm" request này 15 giây
            await Task.Delay(15000);
            return Ok("ok");
        }
    }
}
