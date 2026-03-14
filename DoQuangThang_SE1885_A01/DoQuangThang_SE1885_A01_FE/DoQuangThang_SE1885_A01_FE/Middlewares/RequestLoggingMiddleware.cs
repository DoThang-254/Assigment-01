using System.Diagnostics;

namespace DoQuangThang_SE1885_A01_FE.Middlewares
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            await _next(context); // Chờ hệ thống xử lý xong request

            stopwatch.Stop();
            _logger.LogInformation(
                "API Request: {Method} {Path} | Status: {StatusCode} | Time: {ElapsedMilliseconds}ms",
                context.Request.Method, context.Request.Path, context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
        }
    }
}
