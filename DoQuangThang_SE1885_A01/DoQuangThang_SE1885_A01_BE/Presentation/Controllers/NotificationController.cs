using BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CoreAPI.Controllers
{
    [Route("api/[controller]")]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet("recent")]
        public async Task<IActionResult> GetNotifications()
        {
            var notifications = await _notificationService.GetAllNotifications();
            if (notifications == null || !notifications.Any())
            {
                return NotFound("No notifications found.");
            }

            return Ok(notifications);
        }
    }
}
