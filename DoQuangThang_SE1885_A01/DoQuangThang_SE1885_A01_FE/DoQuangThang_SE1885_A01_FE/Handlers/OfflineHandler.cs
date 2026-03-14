using DoQuangThang_SE1885_A01_FE.Helper;
using System.Net;
using System.Net.Http.Headers;

namespace DoQuangThang_SE1885_A01_FE.Handlers
{
    public class OfflineHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OfflineHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var cacheKey = request.RequestUri.ToString();

            try
            {
                // 1. Gọi API thật (Online)
                var response = await base.SendAsync(request, cancellationToken);

                // Nếu OK và là method GET -> Lưu Cache nhưng KHÔNG ĐỢI
                if (response.IsSuccessStatusCode && request.Method == HttpMethod.Get)
                {
                    // Đọc data vào RAM ngay lập tức
                    var contentBytes = await response.Content.ReadAsByteArrayAsync();
                    var contentType = response.Content.Headers.ContentType;

                    // QUAN TRỌNG: Gắn lại data vào response để luồng chính (Frontend) có thể đọc tiếp mà không bị lỗi stream
                    response.Content = new ByteArrayContent(contentBytes);
                    if (contentType != null)
                    {
                        response.Content.Headers.ContentType = contentType;
                    }

                    // TỐI ƯU TỐC ĐỘ: Fire-and-Forget (Bắn và quên)
                    // Ném việc lưu file ổ cứng cho một luồng ngầm xử lý, KHÔNG AWAIT ở đây!
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await LocalStorageHelper.SaveDataAsync(cacheKey, contentBytes);
                        }
                        catch { /* Bỏ qua lỗi nếu việc lưu file ngầm thất bại để không ảnh hưởng app */ }
                    });

                    // Set Session là Online
                    _httpContextAccessor.HttpContext?.Session.SetString("SystemStatus", "Online");
                }

                // Trả về cho người dùng NGAY LẬP TỨC mà không cần chờ lưu ổ đĩa
                return response;
            }
            //catch (Exception) // Bắt mọi lỗi mất mạng (Socket, DNS, Timeout...)
            //{
            //    // 2. Nếu lỗi -> Vào chế độ Offline (Giữ nguyên logic cực xịn của bạn)
            //    var cachedContent = await LocalStorageHelper.GetDataAsync(cacheKey);

            //    if (cachedContent != null)
            //    {
            //        var fakeResponse = new HttpResponseMessage(HttpStatusCode.OK)
            //        {
            //            Content = new ByteArrayContent(cachedContent),
            //            RequestMessage = request
            //        };

            //        fakeResponse.Headers.Add("X-DataSource", "Offline-Cache");
            //        _httpContextAccessor.HttpContext?.Session.SetString("SystemStatus", "Offline");

            //        return fakeResponse;
            //    }

            //    _httpContextAccessor.HttpContext?.Session.SetString("SystemStatus", "Offline");

            //    return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            //    {
            //        Content = new StringContent("Service is unavailable."),
            //        RequestMessage = request
            //    };
            //}
            catch (Exception ex) when (
    ex is HttpRequestException ||
    ex is TaskCanceledException)
            {
                var cachedContent = await LocalStorageHelper.GetDataAsync(cacheKey);

                if (cachedContent != null)
                {
                    _httpContextAccessor.HttpContext?.Session.SetString("SystemStatus", "Offline");

                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new ByteArrayContent(cachedContent),
                        RequestMessage = request
                    };
                }

                // KHÔNG set offline nếu không có cache
                return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                {
                    Content = new StringContent("Service is unavailable."),
                    RequestMessage = request
                };
            }
        }
    }
}