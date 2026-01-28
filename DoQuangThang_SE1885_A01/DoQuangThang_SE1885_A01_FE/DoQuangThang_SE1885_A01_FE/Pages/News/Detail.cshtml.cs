using DoQuangThang_SE1885_A01_FE.Models.Accounts;
using DoQuangThang_SE1885_A01_FE.Models.News;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace DoQuangThang_SE1885_A01_FE.Pages.News
{
    public class DetailModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _jsonOptions;

        public DetailModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        }

        public NewsDto News { get; set; }

        public List<NewsDto> RelatedArticles { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var client = _httpClientFactory.CreateClient("NewsAPI");

            // 1. Lấy chi tiết bài viết hiện tại
            var response = await client.GetAsync($"api/news('{id}')?$expand=Category,CreatedBy,Tags");
            if (!response.IsSuccessStatusCode) return NotFound();

            var json = await response.Content.ReadAsStringAsync();
            News = JsonSerializer.Deserialize<NewsDto>(json, _jsonOptions);

            // 2. Lấy 3 bài viết liên quan
            if (News != null)
            {
                // Xây dựng query OData cho Related Articles
                // Lọc cùng Category HOẶC chung ít nhất 1 Tag, và phải khác bài hiện tại
                var tagIds = News.Tags?.Select(t => t.TagId).ToList() ?? new List<int>();
                string tagFilter = tagIds.Any()
                    ? $"or Tags/any(t: {string.Join(" or ", tagIds.Select(tid => $"t/TagId eq {tid}"))})"
                    : "";

                var relatedQuery = $"api/news?" +
                    $"$filter=NewsStatus eq true and NewsArticleId ne '{id}' and (" +
                    $"CategoryId eq {News.CategoryId} " +
                    $"{tagFilter})" +
                    $"&$expand=Category&$top=3&$orderby=CreatedDate desc";

                var relatedResponse = await client.GetAsync(relatedQuery);
                if (relatedResponse.IsSuccessStatusCode)
                {
                    var relatedJson = await relatedResponse.Content.ReadAsStringAsync();
                    var odataResult = JsonSerializer.Deserialize<ODataResponse<NewsDto>>(relatedJson, _jsonOptions);
                    RelatedArticles = odataResult?.Value ?? new();
                }
            }

            return Page();
        }
    }
}
