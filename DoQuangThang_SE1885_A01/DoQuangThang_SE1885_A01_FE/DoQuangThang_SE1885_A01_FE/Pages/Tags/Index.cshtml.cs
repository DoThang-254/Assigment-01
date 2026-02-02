using DoQuangThang_SE1885_A01_FE.Models.Accounts;
using DoQuangThang_SE1885_A01_FE.Models.Tags;
using DoQuangThang_SE1885_A01_FE.Pages.News;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using DoQuangThang_SE1885_A01_FE.Hubs;

namespace DoQuangThang_SE1885_A01_FE.Pages.Tags
{
    public class IndexModel : StaffAuthorize
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHubContext<ReportHub> _reportHub;

        public IndexModel(IHttpClientFactory httpClientFactory, IHubContext<ReportHub> reportHub)
        {
            _httpClientFactory = httpClientFactory;
            _reportHub = reportHub;
        }

        public List<TagDto> Tags { get; set; } = new();

        [BindProperty]
        public TagDto CreateTag { get; set; } = new();

        [BindProperty]
        public TagUpdateRequestDto UpdateTag { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Keyword { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 3;
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage => PageNumber > 1;

        public int TotalPages { get; set; }


        public async Task OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient("NewsAPI");

            var skip = (PageNumber - 1) * PageSize;

            var query = new StringBuilder("api/tag?");
            query.Append($"$top={PageSize}&$skip={skip}&$count=true");

            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                query.Append($"&$filter=contains(TagName,'{Keyword}')");
            }

            var response = await client.GetAsync(query.ToString());

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ODataResponse<TagDto>>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                Tags = result?.Value ?? new();

                if (result != null)
                {
                    TotalPages = (int)Math.Ceiling(result.Count / (double)PageSize);
                    HasNextPage = PageNumber < TotalPages;
                }
            }
        }

        // New page handler: proxy articles list for a tag.
        // Called via GET on the same page: ?handler=Articles&tagId=123
        public async Task<IActionResult> OnGetArticlesAsync(int tagId)
        {
            var client = _httpClientFactory.CreateClient("NewsAPI");

            try
            {
                // Use the same route your backend expects (OData-style)
                var response = await client.GetAsync($"api/tag({tagId})/NewsArticles");

                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    // Return the backend status and message so client can show it
                    return StatusCode((int)response.StatusCode, content);
                }

                // Return raw JSON from backend (application/json)
                return Content(content, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }



        public async Task<IActionResult> OnPostCreateAsync()
        {
            var client = _httpClientFactory.CreateClient("NewsAPI");

            var response = await client.PostAsJsonAsync("api/Tag", CreateTag);

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Create tag failed.";
                return RedirectToPage();
            }

            TempData["Success"] = "Tag created successfully.";

            // notify clients that metadata changed (tags)
            await _reportHub.Clients.All.SendAsync("MetadataUpdated", "tag");

            return RedirectToPage();
        }


        public async Task<IActionResult> OnPostUpdateAsync()
        {
            var client = _httpClientFactory.CreateClient("NewsAPI");

            var response = await client.PutAsJsonAsync(
                $"api/Tag({UpdateTag.TagId})", UpdateTag);

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Update tag failed.";
                return RedirectToPage();
            }

            TempData["Success"] = "Tag updated successfully.";

            // notify clients
            await _reportHub.Clients.All.SendAsync("MetadataUpdated", "tag");

            return RedirectToPage();
        }


        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var client = _httpClientFactory.CreateClient("NewsAPI");

            var response = await client.DeleteAsync($"api/Tag({id})");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Cannot delete tag because it is used by news.";
                return RedirectToPage();
            }

            TempData["Success"] = "Tag deleted successfully.";

            // notify clients
            await _reportHub.Clients.All.SendAsync("MetadataUpdated", "tag");

            return RedirectToPage();
        }

    }
}