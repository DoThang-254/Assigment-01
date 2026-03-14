using DoQuangThang_SE1885_A01_FE.Models.Accounts;
using DoQuangThang_SE1885_A01_FE.Pages.Accounts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using ClosedXML.Excel;

namespace DoQuangThang_SE1885_A01_FE.Pages.Reports
{

    public class IndexModel : AdminAuthorizeModel
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

        // Export handler: requests CSV from API and returns it as downloadable file.
        // reportType must be one of: ByCategory, ByAuthor, ByStatus, Summary
        public async Task<IActionResult> OnPostExportAsync(string reportType, DateTime? fromDate, DateTime? toDate)
        {
            if (string.IsNullOrWhiteSpace(reportType))
            {
                TempData["ErrorMessage"] = "Report type is required.";
                return RedirectToPage(new { FromDate = fromDate, ToDate = toDate });
            }

            // Normalize route name (basic safety)
            var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "ByCategory", "ByAuthor", "ByStatus", "Summary" };
            if (!allowed.Contains(reportType))
            {
                TempData["ErrorMessage"] = "Invalid report type.";
                return RedirectToPage(new { FromDate = fromDate, ToDate = toDate });
            }

            var client = _httpClientFactory.CreateClient("NewsAPI");

            var sb = new StringBuilder($"/api/report/{reportType}");
            var hasParams = false;
            if (fromDate.HasValue)
            {
                sb.Append(hasParams ? '&' : '?').Append($"fromDate={fromDate:yyyy-MM-dd}");
                hasParams = true;
            }
            if (toDate.HasValue)
            {
                sb.Append(hasParams ? '&' : '?').Append($"toDate={toDate:yyyy-MM-dd}");
            }

            using var request = new HttpRequestMessage(HttpMethod.Get, sb.ToString());
            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/csv"));

            HttpResponseMessage response;
            try
            {
                response = await client.SendAsync(request);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Export failed: " + ex.Message;
                return RedirectToPage(new { FromDate = fromDate, ToDate = toDate });
            }

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = "Export failed: " + err;
                return RedirectToPage(new { FromDate = fromDate, ToDate = toDate });
            }

            var bytes = await response.Content.ReadAsByteArrayAsync();

            // Create a filename that includes report type and date range
            string fromPart = fromDate?.ToString("yyyyMMdd") ?? "all";
            string toPart = toDate?.ToString("yyyyMMdd") ?? "all";
            var safeName = reportType.ToLowerInvariant() + $"_{fromPart}_{toPart}.csv";

            // Return CSV (Excel can open)
            return File(bytes, "text/csv; charset=utf-8", safeName);
        }

        // New: Export as real Excel (.xlsx) using ClosedXML
        // reportType: "ByCategory", "ByAuthor", "ByStatus", or "Summary"
        public async Task<IActionResult> OnPostExportXlsxAsync(string reportType, DateTime? fromDate, DateTime? toDate)
        {
            if (string.IsNullOrWhiteSpace(reportType))
            {
                TempData["ErrorMessage"] = "Report type is required.";
                return RedirectToPage(new { FromDate = fromDate, ToDate = toDate });
            }

            var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "ByCategory", "ByAuthor", "ByStatus", "Summary" };
            if (!allowed.Contains(reportType))
            {
                TempData["ErrorMessage"] = "Invalid report type.";
                return RedirectToPage(new { FromDate = fromDate, ToDate = toDate });
            }

            var client = _httpClientFactory.CreateClient("NewsAPI");

            var sb = new StringBuilder($"/api/report/{reportType}");
            var hasParams = false;
            if (fromDate.HasValue)
            {
                sb.Append(hasParams ? '&' : '?').Append($"fromDate={fromDate:yyyy-MM-dd}");
                hasParams = true;
            }
            if (toDate.HasValue)
            {
                sb.Append(hasParams ? '&' : '?').Append($"toDate={toDate:yyyy-MM-dd}");
            }

            HttpResponseMessage response;
            try
            {
                // Request JSON (default) so we can build a structured Excel workbook
                response = await client.GetAsync(sb.ToString());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Export failed: " + ex.Message;
                return RedirectToPage(new { FromDate = fromDate, ToDate = toDate });
            }

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = "Export failed: " + err;
                return RedirectToPage(new { FromDate = fromDate, ToDate = toDate });
            }

            // Deserialize JSON into List<ReportDTO> or single ReportDTO for Summary
            List<ReportDTO> rows = new();
            if (string.Equals(reportType, "Summary", StringComparison.OrdinalIgnoreCase))
            {
                var single = JsonSerializer.Deserialize<ReportDTO>(
                    await response.Content.ReadAsStringAsync(), _jsonOptions);
                if (single != null) rows.Add(single);
            }
            else
            {
                rows = JsonSerializer.Deserialize<List<ReportDTO>>(
                    await response.Content.ReadAsStringAsync(), _jsonOptions) ?? new();
            }

            // Build Excel workbook
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add(reportType);

            // Header row
            var headers = new[]
            {
                "ReportId",
                "CategoryId",
                "CreatedById",
                "NewsStatus",
                "TotalArticles",
                "ActiveArticles",
                "InactiveArticles"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(1, i + 1).Value = headers[i];
                ws.Cell(1, i + 1).Style.Font.Bold = true;
            }

            // Data rows
            int r = 2;
            foreach (var item in rows)
            {
                ws.Cell(r, 1).Value = item.ReportId;
                ws.Cell(r, 2).Value = item.CategoryId;
                ws.Cell(r, 3).Value = item.CreatedById;
                ws.Cell(r, 4).Value = item.NewsStatus.HasValue ? (item.NewsStatus.Value ? "Active" : "Inactive") : "";
                ws.Cell(r, 5).Value = item.TotalCount;
                ws.Cell(r, 6).Value = item.ActiveCount;
                ws.Cell(r, 7).Value = item.InactiveCount;
                r++;
            }

            // Adjust columns
            ws.Columns().AdjustToContents();

            // Stream workbook to client
            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            ms.Position = 0;

            string fromPart = fromDate?.ToString("yyyyMMdd") ?? "all";
            string toPart = toDate?.ToString("yyyyMMdd") ?? "all";
            var fileName = $"{reportType}_{fromPart}_{toPart}.xlsx";

            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
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

