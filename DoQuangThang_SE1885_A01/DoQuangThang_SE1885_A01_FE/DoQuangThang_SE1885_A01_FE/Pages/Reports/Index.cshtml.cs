using DoQuangThang_SE1885_A01_FE.Models.Accounts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DoQuangThang_SE1885_A01_FE.Pages.Reports
{

    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _jsonOptions;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        // =======================
        // DATA PROPERTIES
        // =======================
        public List<ReportDTO> ReportByCategory { get; set; } = new();
        public List<ReportDTO> ReportByAuthor { get; set; } = new();
        public List<ReportDTO> ReportByStatus { get; set; } = new();
        public ReportDTO Summary { get; set; } = new();

        // =======================
        // FILTER PROPERTIES
        // =======================
        [BindProperty(SupportsGet = true)]
        public DateTime? FromDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? ToDate { get; set; }

        public async Task OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient("NewsAPI");

            // Tạo query string ngày tháng
            string query = "";
            if (FromDate.HasValue) query += $"fromDate={FromDate:yyyy-MM-dd}&";
            if (ToDate.HasValue) query += $"toDate={ToDate:yyyy-MM-dd}&";
            query = query.TrimEnd('&'); // Xóa dấu & thừa
            string queryString = string.IsNullOrEmpty(query) ? "" : $"?{query}";

            try
            {
                // 1. Report By Category
                var resCat = await client.GetAsync($"api/report/ByCategory{queryString}");
                if (resCat.IsSuccessStatusCode)
                    ReportByCategory = JsonSerializer.Deserialize<List<ReportDTO>>(
                        await resCat.Content.ReadAsStringAsync(), _jsonOptions) ?? new();

                // 2. Report By Author
                var resAuth = await client.GetAsync($"api/report/ByAuthor{queryString}");
                if (resAuth.IsSuccessStatusCode)
                    ReportByAuthor = JsonSerializer.Deserialize<List<ReportDTO>>(
                        await resAuth.Content.ReadAsStringAsync(), _jsonOptions) ?? new();

                // 3. Report By Status
                var resStatus = await client.GetAsync($"api/report/ByStatus{queryString}");
                if (resStatus.IsSuccessStatusCode)
                    ReportByStatus = JsonSerializer.Deserialize<List<ReportDTO>>(
                        await resStatus.Content.ReadAsStringAsync(), _jsonOptions) ?? new();

                // 4. Summary (Trả về 1 Object)
                var resSum = await client.GetAsync($"api/report/Summary{queryString}");
                if (resSum.IsSuccessStatusCode)
                    Summary = JsonSerializer.Deserialize<ReportDTO>(
                        await resSum.Content.ReadAsStringAsync(), _jsonOptions) ?? new();
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu cần (ghi log, v.v)
                // Console.WriteLine(ex.Message);
            }
        }
    }


    public class ReportDTO
    {
        // ==========================================
        // KHÓA PHÂN LOẠI
        // ==========================================

        [JsonPropertyName("categoryId")]
        public int? CategoryId { get; set; }

        // -------------- SỬA DÒNG NÀY ----------------
        // JSON trả về số (1, 2, 3...) nên phải để int?
        [JsonPropertyName("createdById")]
        public int? CreatedById { get; set; }
        // --------------------------------------------

        [JsonPropertyName("newsStatus")]
        public bool? NewsStatus { get; set; }

        // ==========================================
        // SỐ LIỆU THỐNG KÊ
        // ==========================================

        [JsonPropertyName("totalArticles")]
        public int TotalCount { get; set; }

        [JsonPropertyName("activeArticles")]
        public int ActiveCount { get; set; }

        [JsonPropertyName("inactiveArticles")]
        public int InactiveCount { get; set; }

        // Thêm field này để map cho đủ JSON (tránh warning)
        [JsonPropertyName("reportId")]
        public int ReportId { get; set; }
    }
}

