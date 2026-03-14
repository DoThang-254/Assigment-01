using DoQuangThang_SE1885_A01_FE.Models;
using DoQuangThang_SE1885_A01_FE.Models.Accounts;
using DoQuangThang_SE1885_A01_FE.Models.Analytics; 
using DoQuangThang_SE1885_A01_FE.Models.Categories;
using DoQuangThang_SE1885_A01_FE.Models.News;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace DoQuangThang_SE1885_A01_FE.Pages.Analytics
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly IMemoryCache _cache;

        public IndexModel(IHttpClientFactory httpClientFactory, IMemoryCache cache)
        {
            _httpClientFactory = httpClientFactory;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            _cache = cache;
        }

        // --- Properties hiển thị dữ liệu ---
        public DashboardDto DashboardData { get; set; } = new();
        public List<TrendingArticleDto> TrendingArticles { get; set; } = new();

        // --- Properties cho Dropdown List ---
        public List<CategoryDto> Categories { get; set; } = new();
        public List<AuthorDto> Authors { get; set; } = new();

        // --- Filter Properties ---
        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public short? CategoryId { get; set; }

        [BindProperty(SupportsGet = true)]
        public short? AuthorId { get; set; }

        public async Task OnGetAsync()
        {
            // 1. Tạo Client
            var clientAnalytics = _httpClientFactory.CreateClient("AnalyticsAPI");
            var clientContent = _httpClientFactory.CreateClient("NewsAPI");
            var queryParams = new List<string>();

            if (StartDate.HasValue) queryParams.Add($"startDate={StartDate:yyyy-MM-dd}");
            if (EndDate.HasValue) queryParams.Add($"endDate={EndDate:yyyy-MM-dd}");
            if (CategoryId.HasValue) queryParams.Add($"categoryId={CategoryId}");
            if (AuthorId.HasValue) queryParams.Add($"authorId={AuthorId}");

            string queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";

            try
            {
                var taskDashboard = clientAnalytics.GetAsync($"api/analytics/dashboard{queryString}");

                var taskTrending = clientAnalytics.GetAsync("api/analytics/trending");

                var taskCats = clientContent.GetAsync("api/category");

                var taskAuths = clientContent.GetAsync("authors");

                // Chờ tất cả xong
                await Task.WhenAll(taskDashboard, taskTrending, taskCats, taskAuths);

                // 4. Đọc dữ liệu
                var dashRes = await taskDashboard;
                var trendRes = await taskTrending;
                var catRes = await taskCats;
                var authRes = await taskAuths;

                // Deserialize Dashboard
                if (dashRes.IsSuccessStatusCode)
                {
                    DashboardData = JsonSerializer.Deserialize<DashboardDto>(
                        await dashRes.Content.ReadAsStringAsync(), _jsonOptions) ?? new();
                }

                // Deserialize Trending
                if (trendRes.IsSuccessStatusCode)
                {
                    TrendingArticles = JsonSerializer.Deserialize<List<TrendingArticleDto>>(
                        await trendRes.Content.ReadAsStringAsync(), _jsonOptions) ?? new();
                }

                // Deserialize Categories (Dropdown)
                if (catRes.IsSuccessStatusCode)
                {
                    var content = await catRes.Content.ReadAsStringAsync();

                    // SỬA ĐỔI: Dùng ODataResponse để hứng
                    var odataResult = JsonSerializer.Deserialize<ODataResponse<CategoryDto>>(content, _jsonOptions);

                    // Lấy list từ property .Value
                    Categories = odataResult?.Value ?? new List<CategoryDto>();
                }

                // Deserialize Authors (Dropdown)
                if (authRes.IsSuccessStatusCode)
                {
                    Authors = JsonSerializer.Deserialize<List<AuthorDto>>(
                        await authRes.Content.ReadAsStringAsync(), _jsonOptions) ?? new();
                }
            }
            catch (Exception)
            {
                // Xử lý lỗi (Log)
                DashboardData = new DashboardDto();
            }
        }


        //public async Task OnGetAsync()
        //{
        //    var clientAnalytics = _httpClientFactory.CreateClient("AnalyticsAPI");
        //    var clientContent = _httpClientFactory.CreateClient("NewsAPI");
        //    var queryParams = new List<string>();

        //    if (StartDate.HasValue) queryParams.Add($"startDate={StartDate:yyyy-MM-dd}");
        //    if (EndDate.HasValue) queryParams.Add($"endDate={EndDate:yyyy-MM-dd}");
        //    if (CategoryId.HasValue) queryParams.Add($"categoryId={CategoryId}");
        //    if (AuthorId.HasValue) queryParams.Add($"authorId={AuthorId}");

        //    string queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";

        //    try
        //    {
        //        // ==========================================
        //        // PHẦN 1: GỌI API REAL-TIME CHO DASHBOARD (Không Cache)
        //        // ==========================================
        //        var taskDashboard = clientAnalytics.GetAsync($"api/analytics/dashboard{queryString}");
        //        var taskTrending = clientAnalytics.GetAsync("api/analytics/trending");

        //        await Task.WhenAll(taskDashboard, taskTrending);

        //        var dashRes = await taskDashboard;
        //        if (dashRes.IsSuccessStatusCode)
        //        {
        //            DashboardData = JsonSerializer.Deserialize<DashboardDto>(
        //                await dashRes.Content.ReadAsStringAsync(), _jsonOptions) ?? new();
        //        }

        //        var trendRes = await taskTrending;
        //        if (trendRes.IsSuccessStatusCode)
        //        {
        //            TrendingArticles = JsonSerializer.Deserialize<List<TrendingArticleDto>>(
        //                await trendRes.Content.ReadAsStringAsync(), _jsonOptions) ?? new();
        //        }

        //        // ==========================================
        //        // PHẦN 2: DÙNG CACHE CHO DROPDOWN LIST (Tối ưu hiệu năng)
        //        // ==========================================

        //        // 2.1 Cache Categories
        //        if (!_cache.TryGetValue("CategoriesListCache", out List<CategoryDto> cachedCats))
        //        {
        //            var catRes = await clientContent.GetAsync("api/category");
        //            if (catRes.IsSuccessStatusCode)
        //            {
        //                var content = await catRes.Content.ReadAsStringAsync();
        //                var odataResult = JsonSerializer.Deserialize<ODataResponse<CategoryDto>>(content, _jsonOptions);
        //                cachedCats = odataResult?.Value ?? new List<CategoryDto>();

        //                // Lưu vào RAM Cache 30 phút
        //                _cache.Set("CategoriesListCache", cachedCats, TimeSpan.FromMinutes(30));
        //            }
        //        }
        //        Categories = cachedCats ?? new List<CategoryDto>();

        //        // 2.2 Cache Authors
        //        if (!_cache.TryGetValue("AuthorsListCache", out List<AuthorDto> cachedAuths))
        //        {
        //            var authRes = await clientContent.GetAsync("authors");
        //            if (authRes.IsSuccessStatusCode)
        //            {
        //                cachedAuths = JsonSerializer.Deserialize<List<AuthorDto>>(
        //                    await authRes.Content.ReadAsStringAsync(), _jsonOptions);

        //                // Lưu vào RAM Cache 30 phút
        //                _cache.Set("AuthorsListCache", cachedAuths ?? new(), TimeSpan.FromMinutes(30));
        //            }
        //        }
        //        Authors = cachedAuths ?? new List<AuthorDto>();
        //    }
        //    catch (Exception)
        //    {
        //        DashboardData = new DashboardDto();
        //    }
        //}

        public IActionResult OnPostExport()
        {
            string baseUrl = "https://localhost:7078/api/analytics/export";

            // Xây dựng query string cho Export tương tự như Dashboard
            var queryParams = new List<string>();
            if (StartDate.HasValue) queryParams.Add($"startDate={StartDate:yyyy-MM-dd}");
            if (EndDate.HasValue) queryParams.Add($"endDate={EndDate:yyyy-MM-dd}");
            if (CategoryId.HasValue) queryParams.Add($"categoryId={CategoryId}");
            if (AuthorId.HasValue) queryParams.Add($"authorId={AuthorId}");

            string queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";

            return Redirect(baseUrl + queryString);
        }
    }
}