using BusinessLogic.Dto;
using BusinessLogic.Services.Implementations;
using BusinessLogic.Services.Interfaces;
using CoreAPI.Hubs;
using DataAccess.Models;
using DataAccess.Models.Dto;
using DataAccess.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Presentation.Controllers
{
    //[Route("api/[controller]")]
    //[ApiController]
    public class newsController : ODataController
    {
        private readonly INewsArticleService _newsArticleService;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IWebHostEnvironment _env;

        public newsController(INewsArticleService newsArticleService, IHubContext<NotificationHub> hubContext, INotificationService notificationService,
            IWebHostEnvironment env)
        {
            _newsArticleService = newsArticleService;
            _hubContext = hubContext;
            _notificationService = notificationService;
            _env = env;
        }

        [EnableQuery]
        public IActionResult Get()
        {
            var rs = _newsArticleService.GetAllNews();
            return Ok(rs);
        }

        [EnableQuery]
        public IActionResult Get(string key)
        {
            try
            {
                var result = _newsArticleService.GetNewsById(key);

                if (result == null)
                {
                    return NotFound();
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    error = "Lỗi xử lý",
                    detail = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }


        //[HttpPost]
        public async Task<IActionResult> Post([FromBody] NewsDto request)
        {
            try
            {
                // 1. Nên thêm await nếu Service hỗ trợ async (để đảm bảo lưu xong mới báo)
                // Nếu Service chưa có Async thì giữ nguyên, nhưng đảm bảo nó không throw Exception
                _newsArticleService.AddNews(request);

                var notification = new Notification
                {
                    Content = $"New article published: {request.NewsTitle}",
                    CreatedAt = DateTime.Now
                };

                // Lưu notification
                await _notificationService.AddNotification(notification);

                // 2. LOG DEBUG: Để biết chắc chắn code chạy đến đây
                Console.WriteLine($"[DEBUG] Đang gửi SignalR: {notification.Content}");

                // 3. Gửi SignalR (Đúng tên sự kiện ReceiveNewArticle)
                if (_hubContext != null)
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveNewArticle", new
                    {
                        msg = notification.Content,
                        date = notification.CreatedAt
                    });
                }
                else
                {
                    Console.WriteLine("[ERROR] _hubContext bị NULL!");
                }

                return Ok(request);
            }
            catch (Exception ex)
            {
                // Log lỗi ra console server để xem
                Console.WriteLine("[ERROR] Lỗi Post News: " + ex.Message);
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("api/news/UploadImage")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { error = "Không tìm thấy file tải lên." });

                // Validate dung lượng (VD: 5MB)
                if (file.Length > 5 * 1024 * 1024)
                    return BadRequest(new { error = "Kích thước file vượt quá 5MB." });

                // Validate định dạng
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                    return BadRequest(new { error = "Định dạng ảnh không hợp lệ." });

                // Tạo tên file ngẫu nhiên và đường dẫn lưu
                var newFileName = $"{Guid.NewGuid()}{extension}";
                var uploadFolder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "images");

                if (!Directory.Exists(uploadFolder))
                    Directory.CreateDirectory(uploadFolder);

                var filePath = Path.Combine(uploadFolder, newFileName);

                // Lưu file vật lý xuống ổ cứng server
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var request = HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";

                return Ok(new { url = $"{baseUrl}/images/{newFileName}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPatch]
        public IActionResult Patch([FromBody] NewsDto request)
        {
            try
            {
                _newsArticleService.UpdateNews(request);
                return Ok(request);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    error = ex.Message
                });
            }
        }

        [HttpDelete]
        public IActionResult Delete(string key)
        {
            try
            {
                _newsArticleService.DeleteNews(key);
                return Ok(new
                {
                    message = "News article deleted successfully."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    error = ex.Message
                });
            }
        }

        [EnableQuery]
        [HttpGet("authors")]
        [ResponseCache(Duration = 300)]
        public IQueryable<AuthorDto> GetAuthors()
        {
            return _newsArticleService.GetAuthorDtos();
        }

    }
}
