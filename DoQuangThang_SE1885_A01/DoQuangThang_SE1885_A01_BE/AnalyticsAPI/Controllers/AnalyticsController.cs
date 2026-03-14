using BusinessLogic.Dto.Analytics;
using BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging; // Cần thêm cái này để log lỗi

namespace AnalyticsAPI.Controllers
{
    // Thêm Route attribute để chuẩn RESTful nếu cần, ví dụ:
    [Route("api/[controller]")]
    public class AnalyticsController : Controller
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ILogger<AnalyticsController> _logger; // Thêm Logger

        public AnalyticsController(IAnalyticsService analyticsService, ILogger<AnalyticsController> logger)
        {
            _analyticsService = analyticsService;
            _logger = logger;
        }

        // 1. Lấy thống kê Dashboard
        [HttpGet("dashboard")]
        [ResponseCache(Duration = 300)]
        public async Task<IActionResult> GetDashboard([FromQuery] DashboardFilterDto filter)
        {
            try
            {
                var data = await _analyticsService.GetDashboardStatsAsync(filter);
                return Ok(data);
            }
            catch (Exception ex)
            {
                // Ghi log lỗi ra console hoặc file
                _logger.LogError(ex, "Error getting dashboard stats.");

                // Trả về lỗi 500 Internal Server Error kèm thông báo chung chung (để bảo mật)
                return StatusCode(500, new { message = "An error occurred while fetching dashboard data." });
            }
        }

        // 2. Lấy danh sách Trending
        [HttpGet("trending")]
        [ResponseCache(Duration = 300)]
        public async Task<IActionResult> GetTrending()
        {
            try
            {
                var data = await _analyticsService.GetTrendingArticlesAsync(10); // Top 10
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trending articles.");
                return StatusCode(500, new { message = "An error occurred while fetching trending articles." });
            }
        }

        // 3. Xuất báo cáo Excel
        [HttpGet("export")]
        public async Task<IActionResult> ExportExcel([FromQuery] DashboardFilterDto filter)
        {
            try
            {
                var fileContent = await _analyticsService.ExportNewsReportAsync(filter);

                // Kiểm tra nếu service trả về null hoặc mảng rỗng (tùy logic service của bạn)
                if (fileContent == null || fileContent.Length == 0)
                {
                    return BadRequest("No data available to export.");
                }

                string fileName = $"NewsReport_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";

                return File(
                    fileContent,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting excel report.");
                // Với lỗi export, đôi khi trả về BadRequest kèm message lỗi cụ thể cũng được
                return BadRequest(new { message = $"Export failed: {ex.Message}" });
            }
        }

        // 4. Gợi ý bài viết (Recommend)
        //[HttpGet("/api/recommend/{id}")]
        //public async Task<IActionResult> GetRecommend(string id)
        //{
        //    if (string.IsNullOrEmpty(id))
        //    {
        //        return BadRequest("Article ID is required.");
        //    }

        //    try
        //    {
        //        var data = await _analyticsService.GetRelatedArticlesAsync(id, 5);

        //        // (Tùy chọn) Nếu không tìm thấy bài gốc, có thể trả về NotFound
        //        if (data == null)
        //        {
        //            return NotFound($"Article with ID {id} not found.");
        //        }

        //        return Ok(data);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"Error getting recommendations for article {id}.");
        //        return StatusCode(500, new { message = "An error occurred while fetching recommendations." });
        //    }
        //}
    }
}