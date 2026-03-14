using DoQuangThang_SE1885_A01_FE.Models.AuditLog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace DoQuangThang_SE1885_A01_FE.Pages.AuditLog
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // List chứa dữ liệu để hiển thị
        public List<AuditLogDTO> AuditLogs { get; set; } = new List<AuditLogDTO>();

        // Binding các ô search từ form
        [BindProperty(SupportsGet = true)]
        public string SearchEmail { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchEntity { get; set; }

        public async Task OnGetAsync()
        {
            // 1. Tạo Client
            var client = _httpClientFactory.CreateClient("NewsAPI");

            // 2. Xây dựng URL với Query String (nếu có search)
            var url = "api/AuditLog";
            var queryParams = new List<string>();

            if (!string.IsNullOrEmpty(SearchEmail))
                queryParams.Add($"email={SearchEmail}");

            if (!string.IsNullOrEmpty(SearchEntity))
                queryParams.Add($"entityName={SearchEntity}");

            if (queryParams.Any())
                url += "?" + string.Join("&", queryParams);

            // 3. Gọi API
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                // 4. Deserialize JSON thành List Object
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true // Quan trọng: bỏ qua phân biệt hoa thường
                };
                AuditLogs = JsonSerializer.Deserialize<List<AuditLogDTO>>(content, options);
            }
        }
    }
}
