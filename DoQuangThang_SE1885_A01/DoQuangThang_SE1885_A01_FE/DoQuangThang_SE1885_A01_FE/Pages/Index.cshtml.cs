using DoQuangThang_SE1885_A01_FE.Models.Accounts;
using DoQuangThang_SE1885_A01_FE.Models.Categories;
using DoQuangThang_SE1885_A01_FE.Models.News;
using DoQuangThang_SE1885_A01_FE.Models.Tags;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace DoQuangThang_SE1885_A01_FE.Pages
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

        // --- Data Properties ---
        public List<NewsDto> NewsList { get; set; } = new();
        public List<CategoryDto> Categories { get; set; } = new();
        public List<AuthorDto> Authors { get; set; } = new(); // Để đổ vào Dropdown Modal


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

        [BindProperty(SupportsGet = true)]
        public string? CategoryName { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? CreatedByID { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SortBy { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? TagName { get; set; }

        [BindProperty(SupportsGet = true)]
        public short? AuthorId { get; set; }

        public async Task OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient("NewsAPI");

            // ===== Load Tags (không đổi) =====
            var tagResponse = await client.GetAsync("api/tag");
            var categoryResponse = await client.GetAsync("api/category");
            var authorResponse = await client.GetAsync("authors");
            if (categoryResponse.IsSuccessStatusCode)
            {
                var categoryJson = await categoryResponse.Content.ReadAsStringAsync();
                var odataResult = JsonSerializer.Deserialize<ODataResponse<CategoryDto>>(
                    categoryJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
                Categories = odataResult?.Value ?? new List<CategoryDto>();
            }

            if (tagResponse.IsSuccessStatusCode)
            {
                var tagJson = await tagResponse.Content.ReadAsStringAsync();
                var odataResult = JsonSerializer.Deserialize<ODataResponse<TagDto>>(
                    tagJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
                AllTags = odataResult?.Value ?? new List<TagDto>();
            }

            if (authorResponse.IsSuccessStatusCode)
            {
                var authorJson = await authorResponse.Content.ReadAsStringAsync();

                Authors = JsonSerializer.Deserialize<List<AuthorDto>>(
                    authorJson,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }
                ) ?? new List<AuthorDto>();
            }


            // ===== Base query =====
            var query = new StringBuilder("api/news?$expand=Category,CreatedBy,Tags&$count=true");

            var filters = new List<string>();

            // 2. THÊM: Đưa điều kiện mặc định vào list filters
            // (Nếu bạn muốn luôn chỉ lấy bài Active)
            filters.Add("NewsStatus eq true");

            // =====================================================
            // 1. GLOBAL KEYWORD SEARCH
            // =====================================================
            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                // Nên dùng Uri.EscapeDataString để xử lý ký tự đặc biệt và dấu cách
                string k = Uri.EscapeDataString(Keyword.Trim());

                // Mẹo: Dùng tolower() để search không phân biệt hoa thường (nếu DB hỗ trợ)
                filters.Add(
                    $"(contains(tolower(NewsTitle),tolower('{k}')) " +
                    $"or contains(tolower(Headline),tolower('{k}')) " +
                    $"or contains(tolower(NewsContent),tolower('{k}')))"
                );
            }

            // =====================================================
            // 2. FILTER BY CATEGORY NAME
            // =====================================================
            if (!string.IsNullOrEmpty(CategoryName))
            {
                filters.Add($"Category/CategoryName eq '{CategoryName}'");
            }

            // =====================================================
            // 3. FILTER BY AUTHOR (AuthorId)
            // =====================================================
            if (AuthorId.HasValue)
            {
                filters.Add($"CreatedById eq {AuthorId.Value}");
            }

            // =====================================================
            // 4. FILTER BY TAG NAME
            // =====================================================
            if (!string.IsNullOrEmpty(TagName))
            {
                filters.Add($"Tags/any(t: t/TagName eq '{TagName}')");
            }

            // =====================================================
            // 5. FILTER BY CREATED DATE
            // =====================================================
            if (StartDate.HasValue)
            {
                filters.Add($"CreatedDate ge {StartDate.Value:yyyy-MM-ddTHH:mm:ss}Z");
            }

            if (EndDate.HasValue)
            {
                filters.Add($"CreatedDate le {EndDate.Value.AddDays(1):yyyy-MM-ddTHH:mm:ss}Z");
            }

            // 3. APPLY FILTER: Chỉ append $filter MỘT LẦN duy nhất
            if (filters.Any())
            {
                // Nối tất cả điều kiện bằng " and "
                query.Append("&$filter=" + string.Join(" and ", filters));
            }

            // =====================================================
            // 6. SORT
            // =====================================================
            if (!string.IsNullOrEmpty(SortBy))
            {
                query.Append($"&$orderby={SortBy}");
            }
            else
            {
                query.Append("&$orderby=CreatedDate desc");
            }

            // =====================================================
            // 7. PAGING
            // =====================================================
            if (CurrentPage < 1) CurrentPage = 1;
            int skip = (CurrentPage - 1) * PageSize;
            query.Append($"&$skip={skip}&$top={PageSize}");

            // ===== Call API =====
            // Debug: Đặt breakpoint ở đây để copy URL kiểm tra trên Postman
            string finalUrl = query.ToString();

            var response = await client.GetAsync(finalUrl);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var odataResult = JsonSerializer.Deserialize<ODataResponse<NewsDto>>(json, _jsonOptions);
                NewsList = odataResult?.Value ?? new();
                TotalItems = odataResult?.Count ?? 0;
            }
        }

    }
}
