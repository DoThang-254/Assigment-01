using BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuditLogController : Controller
    {
        private readonly IAuditService _service;
        public AuditLogController(IAuditService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string? email, [FromQuery] string? entityName)
        {
            var logs = await _service.GetLogs(email, entityName);
            return Ok(logs);
        }
    }
}
