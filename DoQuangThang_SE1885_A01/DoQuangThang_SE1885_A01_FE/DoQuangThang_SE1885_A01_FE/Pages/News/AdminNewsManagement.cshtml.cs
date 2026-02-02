using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DoQuangThang_SE1885_A01_FE.Pages.News
{
    public class AdminNewsManagementModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _jsonOptions;

        public AdminNewsManagementModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public List<NewsDto> NewsList { get; set; } = new();

        public async Task OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient("NewsAPI");

            try
            {
                // Request the API and expand Category and CreatedBy navigation properties.
                var res = await client.GetAsync("api/news?$expand=Category,CreatedBy");
                if (!res.IsSuccessStatusCode)
                {
                    // keep list empty on failure
                    return;
                }

                var json = await res.Content.ReadAsStringAsync();

                // API returns OData wrapper with "value": [...]
                var odata = JsonSerializer.Deserialize<ODataResponse<NewsDto>>(json, _jsonOptions);
                NewsList = odata?.Value ?? new List<NewsDto>();

                // Cache accounts we already looked up to avoid repeated calls
                var accountEmailCache = new Dictionary<short, string?>();

                async Task<string?> ResolveAccountEmailAsync(short id)
                {
                    if (accountEmailCache.TryGetValue(id, out var cached)) return cached;

                    try
                    {
                        // Try OData-style route first, then fallback
                        var accRes = await client.GetAsync($"api/account({id})");
                        if (!accRes.IsSuccessStatusCode)
                        {
                            accRes = await client.GetAsync($"api/account/{id}");
                        }

                        if (accRes.IsSuccessStatusCode)
                        {
                            var accJson = await accRes.Content.ReadAsStringAsync();
                            var acc = JsonSerializer.Deserialize<AccountDto>(accJson, _jsonOptions);
                            accountEmailCache[id] = acc?.AccountEmail ?? acc?.Email;
                            return accountEmailCache[id];
                        }
                    }
                    catch
                    {
                        // ignore
                    }

                    accountEmailCache[id] = null;
                    return null;
                }

                // Resolve CreatedBy and UpdatedBy emails where only Ids are returned
                foreach (var n in NewsList)
                {
                    if (n.CreatedBy == null && n.CreatedById.HasValue)
                    {
                        try
                        {
                            n.CreatedByEmail = await ResolveAccountEmailAsync(n.CreatedById.Value);
                        }
                        catch
                        {
                            // ignore
                        }
                    }

                    if (n.UpdatedById.HasValue)
                    {
                        try
                        {
                            n.UpdatedByEmail = await ResolveAccountEmailAsync(n.UpdatedById.Value);
                        }
                        catch
                        {
                            // ignore
                        }
                    }
                }
            }
            catch
            {
                // swallow and keep empty list to avoid breaking admin UI
            }
        }

        // OData wrapper
        public class ODataResponse<T>
        {
            [JsonPropertyName("value")]
            public List<T> Value { get; set; } = new();
        }

        // DTOs matching API response (includes expanded Category and CreatedBy)
        public class NewsDto
        {
            [JsonPropertyName("newsArticleId")]
            public string NewsArticleId { get; set; } = null!;

            [JsonPropertyName("newsTitle")]
            public string? NewsTitle { get; set; }

            [JsonPropertyName("headline")]
            public string? Headline { get; set; }

            [JsonPropertyName("createdDate")]
            public DateTime? CreatedDate { get; set; }

            [JsonPropertyName("newsStatus")]
            public bool? NewsStatus { get; set; }

            // Category expanded object (when using $expand=Category)
            [JsonPropertyName("category")]
            public CategoryDto? Category { get; set; }

            // CreatedBy expanded object (when using $expand=CreatedBy)
            [JsonPropertyName("createdBy")]
            public AccountDto? CreatedBy { get; set; }

            // API may also return the raw ids
            [JsonPropertyName("categoryId")]
            public short? CategoryId { get; set; }

            [JsonPropertyName("createdById")]
            public short? CreatedById { get; set; }

            [JsonPropertyName("updatedById")]
            public short? UpdatedById { get; set; }

            // Populated after account lookup (when expanded object is not present)
            [JsonIgnore]
            public string? CreatedByEmail { get; set; }

            [JsonIgnore]
            public string? UpdatedByEmail { get; set; }
        }

        public class CategoryDto
        {
            [JsonPropertyName("categoryId")]
            public short? CategoryId { get; set; }

            // API field name for the category display value
            [JsonPropertyName("categoryName")]
            public string? CategoryName { get; set; }
        }

        public class AccountDto
        {
            // account id property name (change if your API uses a different name)
            [JsonPropertyName("systemAccountId")]
            public short? SystemAccountId { get; set; }

            // API field that contains the account email for display
            [JsonPropertyName("accountEmail")]
            public string? AccountEmail { get; set; }

            // fallback/common name
            [JsonPropertyName("email")]
            public string? Email { get; set; }

            [JsonPropertyName("fullName")]
            public string? FullName { get; set; }
        }
    }
}
