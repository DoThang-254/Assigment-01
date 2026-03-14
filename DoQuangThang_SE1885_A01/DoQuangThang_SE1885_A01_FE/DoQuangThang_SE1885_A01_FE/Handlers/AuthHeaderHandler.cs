using System.Net;
using System.Net.Http.Headers;

namespace DoQuangThang_SE1885_A01_FE.Handlers
{
    public class AuthHeaderHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuthHeaderHandler> _logger; // 1. Khai báo Logger

        // 2. Inject Logger vào Constructor
        public AuthHeaderHandler(IHttpContextAccessor httpContextAccessor, ILogger<AuthHeaderHandler> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var requestUrl = request.RequestUri?.ToString();
            _logger.LogInformation($"[AuthHandler] >>> Bắt đầu request tới: {requestUrl}");

            // 1. Lấy Token từ Session
            var session = _httpContextAccessor.HttpContext.Session;
            var accessToken = session.GetString("AccessToken");

            if (!string.IsNullOrEmpty(accessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                _logger.LogInformation($"[AuthHandler] Đã gắn AccessToken vào Header (Token: {accessToken.Substring(0, 10)}...)");
            }
            else
            {
                _logger.LogWarning("[AuthHandler] Không tìm thấy AccessToken trong Session!");
            }

            // 2. Gửi Request đi lần 1
            var response = await base.SendAsync(request, cancellationToken);

            _logger.LogInformation($"[AuthHandler] Nhận phản hồi từ {requestUrl}. Status: {response.StatusCode}");
            _logger.LogWarning($"[DEBUG] API trả về Code: {response.StatusCode}");
            // 3. Nếu gặp lỗi 401 Unauthorized -> Token hết hạn
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                _logger.LogWarning("[AuthHandler] Phát hiện lỗi 401. Đang thử Refresh Token...");

                var refreshToken = session.GetString("RefreshToken");
                if (string.IsNullOrEmpty(refreshToken))
                {
                    _logger.LogError("[AuthHandler] Không có RefreshToken trong Session. Dừng xử lý.");
                    return response;
                }

                // Gọi API Refresh Token
                var newTokens = await CallRefreshTokenApi(accessToken, refreshToken);

                if (newTokens != null)
                {
                    _logger.LogInformation("[AuthHandler] Refresh Token THÀNH CÔNG! Đang cập nhật Session...");

                    // 4. Lưu Token mới vào Session
                    session.SetString("AccessToken", newTokens.AccessToken);
                    session.SetString("RefreshToken", newTokens.RefreshToken);

                    // 5. QUAN TRỌNG: Clone request cũ ra request mới
                    var newRequest = await CloneHttpRequestMessageAsync(request);

                    // Gắn token mới vào header của request mới
                    newRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newTokens.AccessToken);

                    _logger.LogInformation("[AuthHandler] Đang gửi lại Request với Token mới...");

                    // Dispose response cũ (401) để giải phóng resource
                    response.Dispose();

                    var retryResponse = await base.SendAsync(newRequest, cancellationToken);
                    _logger.LogInformation($"[AuthHandler] Kết quả Retry: {retryResponse.StatusCode}");

                    return retryResponse;
                }
                else
                {
                    _logger.LogError("[AuthHandler] Refresh Token THẤT BẠI. Xóa Session và buộc đăng xuất.");
                    session.Clear();
                }
            }

            return response;
        }

        // --- HÀM CLONE REQUEST ---
        private async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage req)
        {
            HttpRequestMessage clone = new HttpRequestMessage(req.Method, req.RequestUri);

            if (req.Content != null)
            {
                var ms = new MemoryStream();
                await req.Content.CopyToAsync(ms);
                ms.Position = 0;
                clone.Content = new StreamContent(ms);

                if (req.Content.Headers != null)
                {
                    foreach (var h in req.Content.Headers)
                    {
                        clone.Content.Headers.TryAddWithoutValidation(h.Key, h.Value);
                    }
                }
            }

            clone.Version = req.Version;

            foreach (var h in req.Headers)
            {
                clone.Headers.TryAddWithoutValidation(h.Key, h.Value);
            }

            return clone;
        }

        private async Task<TokenResponse> CallRefreshTokenApi(string expiredToken, string refreshToken)
        {
            try
            {
                _logger.LogInformation("[AuthHandler] Đang gọi API Refresh Token...");

                // Đảm bảo URL này đúng
                var client = new HttpClient { BaseAddress = new Uri("https://localhost:7066/") };

                var response = await client.PostAsJsonAsync("api/auth/refresh-token", new
                {
                    AccessToken = expiredToken,
                    RefreshToken = refreshToken
                });

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<TokenResponse>();
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"[AuthHandler] API Refresh trả về lỗi: {response.StatusCode} - {error}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[AuthHandler] Lỗi Exception khi gọi API Refresh: {ex.Message}");
            }
            return null;
        }

        private class TokenResponse
        {
            public string AccessToken { get; set; }
            public string RefreshToken { get; set; }
        }
    }
}