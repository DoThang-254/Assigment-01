using DoQuangThang_SE1885_A01_FE.Models.Accounts;
using DoQuangThang_SE1885_A01_FE.Models.Categories;
using DoQuangThang_SE1885_A01_FE.Models.News;
using DoQuangThang_SE1885_A01_FE.Models.Tags;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory; // 1. BẮT BUỘC THÊM THƯ VIỆN NÀY
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
        private readonly IMemoryCache _cache; // 2. KHAI BÁO CACHE

        // 3. INJECT VÀO CONSTRUCTOR
        public IndexModel(IHttpClientFactory httpClientFactory, IMemoryCache cache)
        {
            _httpClientFactory = httpClientFactory;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            _cache = cache;
        }

        // --- Data Properties ---
        public List<NewsDto> NewsList { get; set; } = new();
        public List<CategoryDto> Categories { get; set; } = new();
        public List<AuthorDto> Authors { get; set; } = new();

        // --- Lược bỏ bớt code khai báo Properties cho gọn (Bạn giữ nguyên của bạn nhé) ---
        [BindProperty(SupportsGet = true)] public string Keyword { get; set; }
        [BindProperty(SupportsGet = true)] public string Status { get; set; }
        [BindProperty(SupportsGet = true)] public DateTime? StartDate { get; set; }
        [BindProperty(SupportsGet = true)] public DateTime? EndDate { get; set; }
        [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 5;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling(decimal.Divide(TotalItems, PageSize));
        [BindProperty(SupportsGet = true)] public List<TagDto> AllTags { get; set; }
        [BindProperty(SupportsGet = true)] public string? CategoryName { get; set; }
        [BindProperty(SupportsGet = true)] public int? CreatedByID { get; set; }
        [BindProperty(SupportsGet = true)] public string? SortBy { get; set; }
        [BindProperty(SupportsGet = true)] public string? TagName { get; set; }
        [BindProperty(SupportsGet = true)] public short? AuthorId { get; set; }

        public async Task OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient("NewsAPI");

            // =====================================================
            // TỐI ƯU 1: LẤY DỮ LIỆU TĨNH TỪ RAM CACHE (Thay vì gọi API)
            // =====================================================

            // 1. Cache Categories
            //if (!_cache.TryGetValue("CategoriesListCache", out List<CategoryDto> cachedCats))
            //{
            //    var catRes = await client.GetAsync("api/category");
            //    if (catRes.IsSuccessStatusCode)
            //    {
            //        var content = await catRes.Content.ReadAsStringAsync();
            //        var odataResult = JsonSerializer.Deserialize<ODataResponse<CategoryDto>>(content, _jsonOptions);
            //        cachedCats = odataResult?.Value ?? new List<CategoryDto>();
            //        _cache.Set("CategoriesListCache", cachedCats, TimeSpan.FromMinutes(30));
            //    }
            //}
            //Categories = cachedCats ?? new List<CategoryDto>();

            //// 2. Cache Tags
            //if (!_cache.TryGetValue("TagsListCache", out List<TagDto> cachedTags))
            //{
            //    var tagRes = await client.GetAsync("api/tag");
            //    if (tagRes.IsSuccessStatusCode)
            //    {
            //        var content = await tagRes.Content.ReadAsStringAsync();
            //        var odataResult = JsonSerializer.Deserialize<ODataResponse<TagDto>>(content, _jsonOptions);
            //        cachedTags = odataResult?.Value ?? new List<TagDto>();
            //        _cache.Set("TagsListCache", cachedTags, TimeSpan.FromMinutes(30));
            //    }
            //}
            //AllTags = cachedTags ?? new List<TagDto>();

            //// 3. Cache Authors
            //if (!_cache.TryGetValue("AuthorsListCache", out List<AuthorDto> cachedAuths))
            //{
            //    var authRes = await client.GetAsync("authors");
            //    if (authRes.IsSuccessStatusCode)
            //    {
            //        cachedAuths = JsonSerializer.Deserialize<List<AuthorDto>>(
            //            await authRes.Content.ReadAsStringAsync(), _jsonOptions);
            //        _cache.Set("AuthorsListCache", cachedAuths ?? new(), TimeSpan.FromMinutes(30));
            //    }
            //}
            //Authors = cachedAuths ?? new List<AuthorDto>();
            // 1. Categories (NO CACHE)
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

                // 1. THÊM CHỐT CHẶN Ở ĐÂY: Kiểm tra chuỗi có rỗng không
                if (!string.IsNullOrWhiteSpace(authorJson))
                {
                    try
                    {
                        Authors = JsonSerializer.Deserialize<List<AuthorDto>>(
                            authorJson,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                        ) ?? new List<AuthorDto>();
                    }
                    catch (JsonException ex)
                    {
                        // Bắt lỗi rủi ro nếu API trả về text thuần thay vì JSON
                        Console.WriteLine($"[PARSE ERROR] Authors JSON is invalid: {ex.Message}");
                        Authors = new List<AuthorDto>();
                    }
                }
                else
                {
                    // 2. Nếu chuỗi rỗng thì gán list rỗng luôn cho an toàn
                    Authors = new List<AuthorDto>();
                }
            }

            // =====================================================
            // TỐI ƯU 2: GỌI API NEWS TRỰC TIẾP (Giữ nguyên logic tạo OData)
            // =====================================================
            var query = new StringBuilder("api/news?$expand=Category,CreatedBy,Tags&$count=true");
            var filters = new List<string>();

            // Luôn chỉ lấy bài Active cho khách hàng xem
            filters.Add("NewsStatus eq true");

            // 1. GLOBAL KEYWORD SEARCH
            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                string k = Uri.EscapeDataString(Keyword.Trim());
                filters.Add($"(contains(tolower(NewsTitle),tolower('{k}')) or contains(tolower(Headline),tolower('{k}')) or contains(tolower(NewsContent),tolower('{k}')))");
            }

            // 2. FILTER BY CATEGORY NAME
            if (!string.IsNullOrEmpty(CategoryName))
                filters.Add($"Category/CategoryName eq '{CategoryName}'");

            // 3. FILTER BY AUTHOR (AuthorId)
            if (AuthorId.HasValue)
                filters.Add($"CreatedById eq {AuthorId.Value}");

            // 4. FILTER BY TAG NAME
            if (!string.IsNullOrEmpty(TagName))
                filters.Add($"Tags/any(t: t/TagName eq '{TagName}')");

            // 5. FILTER BY CREATED DATE
            if (StartDate.HasValue)
                filters.Add($"CreatedDate ge {StartDate.Value:yyyy-MM-ddTHH:mm:ss}Z");
            if (EndDate.HasValue)
                filters.Add($"CreatedDate le {EndDate.Value.AddDays(1):yyyy-MM-ddTHH:mm:ss}Z");

            // APPLY FILTER
            if (filters.Any())
                query.Append("&$filter=" + string.Join(" and ", filters));

            // 6. SORT
            if (!string.IsNullOrEmpty(SortBy))
                query.Append($"&$orderby={SortBy}");
            else
                query.Append("&$orderby=CreatedDate desc");

            // 7. PAGING
            if (CurrentPage < 1) CurrentPage = 1;
            int skip = (CurrentPage - 1) * PageSize;
            query.Append($"&$skip={skip}&$top={PageSize}");

            // ===== Call API (Luôn gọi để lấy tin mới nhất) =====
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