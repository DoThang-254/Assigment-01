using DoQuangThang_SE1885_A01_FE.Models.Accounts;
using DoQuangThang_SE1885_A01_FE.Models.Tags;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;

namespace DoQuangThang_SE1885_A01_FE.Pages.Tags
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
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
            return RedirectToPage();
        }

    }
}
