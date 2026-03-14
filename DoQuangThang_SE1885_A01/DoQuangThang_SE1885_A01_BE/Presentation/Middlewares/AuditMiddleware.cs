using DataAccess.Models;
using System.Security.Claims;

namespace CoreAPI.Middlewares
{
    public class AuditMiddleware
    {
        private readonly RequestDelegate _next;

        public AuditMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        // Middleware tự động Inject DbContext của request hiện tại
        public async Task Invoke(HttpContext context, FunewsManagementContext dbContext)
        {
            // Kiểm tra user đã login chưa
            if (context.User.Identity?.IsAuthenticated == true)
            {
                // Lấy Email hoặc ID từ Token (ClaimTypes.Email hoặc NameIdentifier)
                var userId = context.User.FindFirst(ClaimTypes.Email)?.Value
                             ?? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!string.IsNullOrEmpty(userId))
                {
                    // GÁN USER VÀO DBCONTEXT
                    dbContext.CurrentUserId = userId;
                }
            }

            // Cho phép request đi tiếp đến Controller
            await _next(context);
        }
    }
}
