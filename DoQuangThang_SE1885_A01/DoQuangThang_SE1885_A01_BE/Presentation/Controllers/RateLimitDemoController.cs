using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RateLimitDemoController : ControllerBase
    {
        // 1. Fixed Window Policy: Cho phép 5 request mỗi 10 giây (áp dụng cho các controller thông thường).
        [HttpGet("normal-data")]
        [EnableRateLimiting("FixedWindow")]
        public IActionResult GetNormalData()
        {
            return Ok(new
            {
                message = "Truy cập dữ liệu thông thường thành công!",
                timestamp = System.DateTime.Now
            });
        }

        // 2. Token Bucket Policy: Cho phép 100 request, hồi phục 10 token mỗi phút (áp dụng cho các API lấy dữ liệu nặng).
        [HttpGet("heavy-data")]
        [EnableRateLimiting("TokenBucket")]
        public IActionResult GetHeavyData()
        {
            return Ok(new
            {
                message = "Truy cập dữ liệu NẶNG (Token Bucket) thành công!",
                timestamp = System.DateTime.Now
            });
        }
    }
}
