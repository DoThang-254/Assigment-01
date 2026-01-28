using DoQuangThang_SE1885_A01_FE.Models;
using DoQuangThang_SE1885_A01_FE.Models.Accounts;
using DoQuangThang_SE1885_A01_FE.Models.Categories;
using DoQuangThang_SE1885_A01_FE.Models.News;
using DoQuangThang_SE1885_A01_FE.Models.Tags;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace DoQuangThang_SE1885_A01_FE.Pages.News
{
    public class IndexModel : StaffAuthorize
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _jsonOptions;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        // --- Data Properties ---
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

        public async Task OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient("NewsAPI");

            var tagResponse = await client.GetAsync("api/tag");
            var categoryResponse = await client.GetAsync("api/category");

            if (categoryResponse.IsSuccessStatusCode)
            {
                var categoryJson = await categoryResponse.Content.ReadAsStringAsync();

                var odataCategories = JsonSerializer.Deserialize<ODataResponse<CategoryDto>>(
                    categoryJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                Categories = odataCategories?.Value ?? new List<CategoryDto>();
            }
            else
            {
                Categories = new List<CategoryDto>(); 
            }

            if (tagResponse.IsSuccessStatusCode)
            {
                var tagJson = await tagResponse.Content.ReadAsStringAsync();

                // Giải mã ra ODataResponse trước
                var odataResult = JsonSerializer.Deserialize<ODataResponse<TagDto>>(
                    tagJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                // Sau đó mới lấy cái List bên trong ra
                AllTags = odataResult?.Value ?? new List<TagDto>();
            }
            var query = new StringBuilder("api/news?$expand=Category,CreatedBy,Tags&$orderby=CreatedDate desc&$count=true");
            var filters = new List<string>();

            // a. Search Keyword (Title, Author Name, Category Name)
            if (!string.IsNullOrEmpty(Keyword))
            {
                string k = Keyword.Trim();
                filters.Add($"(contains(NewsTitle, '{k}') or contains(Category/CategoryName, '{k}') or contains(CreatedBy/AccountName, '{k}'))");
            }

            // b. Filter Status
            if (!string.IsNullOrEmpty(Status) && bool.TryParse(Status, out bool statusVal))
            {
                filters.Add($"NewsStatus eq {statusVal.ToString().ToLower()}");
            }

            // c. Date Range
            if (StartDate.HasValue)
            {
                // OData date format: YYYY-MM-DDThh:mm:ssZ
                filters.Add($"CreatedDate ge {StartDate.Value:yyyy-MM-ddTHH:mm:ss}Z");
            }
            if (EndDate.HasValue)
            {
                // Cộng thêm 1 ngày để lấy hết ngày cuối cùng
                filters.Add($"CreatedDate le {EndDate.Value.AddDays(1):yyyy-MM-ddTHH:mm:ss}Z");
            }

            // Gép các filter lại bằng "and"
            if (filters.Any())
            {
                query.Append("&$filter=" + string.Join(" and ", filters));
            }

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
        [BindProperty(SupportsGet = true)]
        public List<TagDto> AllTags { get; set; }

        [BindProperty]
        public List<int> TagIds { get; set; }
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