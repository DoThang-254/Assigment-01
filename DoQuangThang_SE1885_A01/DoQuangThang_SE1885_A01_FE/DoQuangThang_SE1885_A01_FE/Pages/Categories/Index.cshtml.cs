using DoQuangThang_SE1885_A01_FE.Models.Accounts;
using DoQuangThang_SE1885_A01_FE.Pages.News;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using DoQuangThang_SE1885_A01_FE.Hubs;

namespace DoQuangThang_SE1885_A01_FE.Pages.Categories
{
    public class IndexModel : StaffAuthorize
    {
        private readonly HttpClient _http;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly IHubContext<ReportHub> _reportHub;

        public IndexModel(IHttpClientFactory factory, IHubContext<ReportHub> reportHub)
        {
            _http = factory.CreateClient("NewsAPI");
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            _reportHub = reportHub;
        }

        public List<CategoryViewDto> Categories { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty]
        public CategoryCreateRequestDto Category { get; set; } = new();

        [BindProperty]
        public CategoryUpdateRequestDto UpdatedCategory { get; set; } = new();

        // LIST + SEARCH


        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1; // Mặc định trang 1

        public int PageSize { get; set; } = 5; // Số dòng mỗi trang
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling(decimal.Divide(TotalItems, PageSize));
        public async Task OnGetAsync()
        {
            // 1. Xử lý Search Filter
            var filters = new List<string>();
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                // Lưu ý: CategoryDesciption của bạn đang thiếu chữ 'r', giữ nguyên theo DB của bạn
                filters.Add($"(contains(CategoryName, '{SearchTerm}') or contains(CategoryDesciption, '{SearchTerm}'))");
            }

            // 2. Xử lý Paging (OData)
            if (CurrentPage < 1) CurrentPage = 1;
            int skip = (CurrentPage - 1) * PageSize;

            var url = $"/api/CountNewsByCategory?$count=true&$skip={skip}&$top={PageSize}&$orderby=CategoryId desc";

            if (filters.Any())
            {
                url += "&$filter=" + string.Join(" and ", filters);
            }

            // 3. Gọi API
            var response = await _http.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                // Deserialize vào ODataResponse wrapper
                var result = JsonSerializer.Deserialize<ODataResponse<CategoryViewDto>>(content, _jsonOptions);

                if (result != null)
                {
                    Categories = result.Value ?? new List<CategoryViewDto>();
                    TotalItems = result.Count;
                }
            }
            else
            {
                Categories = new List<CategoryViewDto>();
            }
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            // Xóa validate của phần Update
            foreach (var key in ModelState.Keys.Where(k => k.StartsWith("UpdatedCategory")).ToList())
            {
                ModelState.Remove(key);
            }

            var response = await _http.PostAsJsonAsync("https://localhost:7066/api/category", Category);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"Failed: {errorContent}";
            }
            else
            {
                TempData["Success"] = "Category created successfully.";

                // notify clients
                await _reportHub.Clients.All.SendAsync("MetadataUpdated", "category");
            }

            return RedirectToPage();
        }

        // ===== 2. LOGIC UPDATE RIÊNG =====
        public async Task<IActionResult> OnPostUpdateAsync()
        {
            foreach (var key in ModelState.Keys.Where(k => k.StartsWith("Category")).ToList())
            {
                ModelState.Remove(key);
            }

            var res = await _http.PutAsJsonAsync(
                $"https://localhost:7066/api/category({UpdatedCategory.CategoryId})",
                UpdatedCategory);

            if (!res.IsSuccessStatusCode)
            {
                TempData["Error"] = "Category is used by articles or update failed.";
            }
            else
            {
                TempData["Success"] = "Category updated successfully.";

                // notify clients
                await _reportHub.Clients.All.SendAsync("MetadataUpdated", "category");
            }
            return RedirectToPage();
        }

        // DELETE
        public async Task<IActionResult> OnGetDeleteAsync(int id)
        {
            var res = await _http.DeleteAsync($"/api/category({id})");

            if (!res.IsSuccessStatusCode)
            {
                TempData["Error"] = "Category is used by articles.";
            }
            else
            {
                // notify clients
                await _reportHub.Clients.All.SendAsync("MetadataUpdated", "category");
            }

            return RedirectToPage();
        }
    }

    // ===== DTO =====

    public class CategoryViewDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public string CategoryDesciption { get; set; } = null!;
        public bool? IsActive { get; set; }
        public int ArticleCount { get; set; }
    }

    public class CategoryCreateRequestDto
    {
        public string CategoryName { get; set; } = null!;
        public string CategoryDescription { get; set; } = null!;
        public short? ParentCategoryId { get; set; }
    }

    public class CategoryUpdateRequestDto
    {
        public short CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public string CategoryDesciption { get; set; } = null!;
        public short? ParentCategoryId { get; set; }
        public bool? IsActive { get; set; }
    }
}