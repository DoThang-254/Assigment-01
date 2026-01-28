using DoQuangThang_SE1885_A01_FE.Models.Accounts;
using DoQuangThang_SE1885_A01_FE.Models.Categories;
using DoQuangThang_SE1885_A01_FE.Models.News;
using DoQuangThang_SE1885_A01_FE.Models.Tags;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace DoQuangThang_SE1885_A01_FE.Pages.News
{
    public class NewsHistoryModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _jsonOptions;
        public NewsHistoryModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }
        public List<NewsDto> NewsList { get; set; } = new();
        public List<CategoryDto> Categories { get; set; } = new(); // Để đổ vào Dropdown Modal

        // --- Search/Filter Properties ---
        [BindProperty(SupportsGet = true)]
        public string Keyword { get; set; } // Search Title, Author, CategoryName

        [BindProperty(SupportsGet = true)]
        public string Status { get; set; } // "true", "false" or empty

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1; // Trang hiện tại
        public int PageSize { get; set; } = 5;      // Số dòng mỗi trang (Tùy chỉnh)
        public int TotalItems { get; set; }         // Tổng số bài viết tìm thấy
        public int TotalPages => (int)Math.Ceiling(decimal.Divide(TotalItems, PageSize));

        [BindProperty(SupportsGet = true)]
        public List<TagDto> AllTags { get; set; }

        [BindProperty]
        public List<int> TagIds { get; set; }

        public async Task OnGetAsync()
        {
            // 1. Lấy AccountId từ Session
            var accountId = HttpContext.Session.GetInt32("AccountId");

            // Nếu không tìm thấy session (chưa đăng nhập), chuyển hướng về Login
            if (accountId == null)
            {
                Response.Redirect("/Index");
                return;
            }

            var client = _httpClientFactory.CreateClient("NewsAPI");

            // ... (Phần code khởi tạo Categories giữ nguyên) ...

            // ... (Phần code lấy Tags giữ nguyên) ...
            var tagResponse = await client.GetAsync("api/tag");
            if (tagResponse.IsSuccessStatusCode)
            {
                var tagJson = await tagResponse.Content.ReadAsStringAsync();
                var odataResult = JsonSerializer.Deserialize<ODataResponse<TagDto>>(
                    tagJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
                AllTags = odataResult?.Value ?? new List<TagDto>();
            }

            // 2. Xây dựng Query OData
            var query = new StringBuilder("api/news?$expand=Category,CreatedBy,Tags&$orderby=CreatedDate desc&$count=true");
            var filters = new List<string>();

            // --- [QUAN TRỌNG] ---
            // Thêm điều kiện lọc theo người tạo (CreatedById) lấy từ Session
            filters.Add($"CreatedById eq {accountId}");
            // --------------------

            // a. Search Keyword (Title, CategoryName)
            // Lưu ý: Đã bỏ check 'CreatedBy/AccountName' vì ta đang xem lịch sử của chính mình, 
            // nên tên người tạo luôn là mình, search thừa thãi.
            if (!string.IsNullOrEmpty(Keyword))
            {
                string k = Keyword.Trim();
                filters.Add($"(contains(NewsTitle, '{k}') or contains(Category/CategoryName, '{k}'))");
            }

            // b. Filter Status
            if (!string.IsNullOrEmpty(Status) && bool.TryParse(Status, out bool statusVal))
            {
                filters.Add($"NewsStatus eq {statusVal.ToString().ToLower()}");
            }

            // c. Date Range
            if (StartDate.HasValue)
            {
                filters.Add($"CreatedDate ge {StartDate.Value:yyyy-MM-ddTHH:mm:ss}Z");
            }
            if (EndDate.HasValue)
            {
                filters.Add($"CreatedDate le {EndDate.Value.AddDays(1):yyyy-MM-ddTHH:mm:ss}Z");
            }

            // Ghép các filter lại bằng "and"
            if (filters.Any())
            {
                query.Append("&$filter=" + string.Join(" and ", filters));
            }

            // Phân trang
            if (CurrentPage < 1) CurrentPage = 1;
            int skip = (CurrentPage - 1) * PageSize;
            query.Append($"&$skip={skip}&$top={PageSize}");

            // 3. Call API
            var response = await client.GetAsync(query.ToString());
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var odataResult = JsonSerializer.Deserialize<ODataResponse<NewsDto>>(json, _jsonOptions);
                NewsList = odataResult?.Value ?? new();
                if (odataResult != null)
                {
                    TotalItems = odataResult.Count;
                }
            }
        }
        public async Task<IActionResult> OnPostSaveAsync(NewsDto news)
        {
            var currentAccountId = HttpContext.Session.GetInt32("AccountId");

            if (!currentAccountId.HasValue)
            {
                TempData["ErrorMessage"] = "You must be logged in to create news.";
                return RedirectToPage("/Index");
            }

            // 1. Xử lý các giá trị mặc định tránh Null

            if (string.IsNullOrEmpty(news.NewsSource))
            {
                news.NewsSource = "System";
            }

            // 2. Gán ID người tạo/sửa
            // Khi Update, API thường cần UpdatedById
            short accountId = (short)currentAccountId.Value;
            news.CreatedById = accountId;
            news.UpdatedById = accountId;
            news.TagIds = this.TagIds ?? new List<int>();
            var client = _httpClientFactory.CreateClient("NewsAPI");

            bool isUpdate = !string.IsNullOrEmpty(news.NewsArticleId) && news.NewsArticleId != "0";

            // 3. --- QUAN TRỌNG NHẤT: TẠO PAYLOAD SẠCH ---
            // Chỉ gửi những dữ liệu API cần. Loại bỏ "Category" và "CreatedBy" (đang là null)
            // để tránh làm API bị crash khi nó cố đọc thuộc tính của null.

            var payload = new
            {
                NewsArticleId = news.NewsArticleId,
                NewsTitle = news.NewsTitle,
                Headline = news.Headline,
                NewsContent = news.NewsContent,
                NewsSource = news.NewsSource,
                CategoryId = news.CategoryId,
                NewsStatus = news.NewsStatus,
                CreatedById = news.CreatedById,
                UpdatedById = news.UpdatedById,
                CreatedDate = news.CreatedDate ?? DateTime.Now,
                TagIds = this.TagIds ?? new List<int>(),
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            if (isUpdate)
            {
                // Lưu ý: Đảm bảo route Update của API đúng (thường cần ID trên URL)
                // Nếu API của bạn là OData Update: api/news('ID')
                // Nếu API thường: api/news hoặc api/news/{id}
                // Dưới đây giả định API thường nhận ID trong Body hoặc query
                var request = new HttpRequestMessage(HttpMethod.Patch, "api/news")
                {
                    Content = jsonContent
                };
                response = await client.SendAsync(request);
            }
            else
            {
                response = await client.PostAsync("api/news", jsonContent);
            }

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = isUpdate ? "News updated successfully!" : "News created successfully!";
            }
            else
            {
                var err = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Action failed: {err}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            var client = _httpClientFactory.CreateClient("NewsAPI");

            var response = await client.DeleteAsync($"api/news('{id}')");

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "News deleted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Delete failed.";
            }

            return RedirectToPage();
        }
    }
}
