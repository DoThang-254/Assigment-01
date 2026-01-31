using DoQuangThang_SE1885_A01_FE.Models.Accounts;
using DoQuangThang_SE1885_A01_FE.Models.News;
using DoQuangThang_SE1885_A01_FE.Models.Categories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Diagnostics;

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

            // 1. Get current news with expanded relations (OData-style)
            var response = await client.GetAsync($"api/news('{id}')?$expand=Category,CreatedBy,Tags");
            if (!response.IsSuccessStatusCode) return NotFound();

            var json = await response.Content.ReadAsStringAsync();

            // Debug: inspect raw JSON returned by API
            Debug.WriteLine("News detail json: " + json);

            // Robust deserialization to handle:
            // - direct entity
            // - OData wrapper { "value": { ... } } or { "value": [ ... ] }
            // - responses where the news object is nested
            News = TryParseNewsFromJson(json);

            // If basic parse didn't yield relations, attempt to fetch them individually
            if (News != null)
            {
                if (News.Category == null && News.CategoryId != 0)
                {
                    try
                    {
                        var catResp = await client.GetAsync($"api/categories({News.CategoryId})");
                        if (catResp.IsSuccessStatusCode)
                        {
                            var catJson = await catResp.Content.ReadAsStringAsync();
                            var cat = TryParseSingle<CategoryDto>(catJson);
                            if (cat != null) News.Category = cat;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Category fetch failed: " + ex);
                    }
                }

                if (News.CreatedBy == null && News.CreatedById.HasValue)
                {
                    try
                    {
                        var accResp = await client.GetAsync($"api/accounts({News.CreatedById.Value})");
                        if (accResp.IsSuccessStatusCode)
                        {
                            var accJson = await accResp.Content.ReadAsStringAsync();
                            var acc = TryParseSingle<AccountsDto>(accJson);
                            if (acc != null) News.CreatedBy = acc;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("CreatedBy fetch failed: " + ex);
                    }
                }
            }

            if (News == null)
            {
                Debug.WriteLine("Failed to deserialize News detail or expanded properties missing.");
                return Page();
            }

            // 2. Get related articles (same category or sharing tags), exclude current article
            var tagIds = News.Tags?.Select(t => t.TagId).ToList() ?? new List<int>();
            string tagFilter = tagIds.Any()
                ? $" or Tags/any(t: {string.Join(" or ", tagIds.Select(tid => $"t/TagId eq {tid}"))})"
                : string.Empty;

            var relatedQuery = $"api/news?" +
                $"$filter=NewsStatus eq true and NewsArticleId ne '{id}' and (" +
                $"CategoryId eq {News.CategoryId}{tagFilter})" +
                $"&$expand=Category&$top=3&$orderby=CreatedDate desc";

            var relatedResponse = await client.GetAsync(relatedQuery);
            if (relatedResponse.IsSuccessStatusCode)
            {
                var relatedJson = await relatedResponse.Content.ReadAsStringAsync();
                RelatedArticles = TryParseListFromOData<NewsDto>(relatedJson) ?? new List<NewsDto>();
            }

            return Page();
        }

        private NewsDto TryParseNewsFromJson(string json)
        {
            // Try direct deserialization first
            try
            {
                var direct = JsonSerializer.Deserialize<NewsDto>(json, _jsonOptions);
                if (direct != null && !string.IsNullOrEmpty(direct.NewsArticleId)) return direct;
            }
            catch { /* ignore and try other forms */ }

            // Use JsonDocument to handle wrappers like { "value": { ... } } or { "value": [ ... ] }
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // OData style "value"
                if (root.TryGetProperty("value", out var valueProp))
                {
                    if (valueProp.ValueKind == JsonValueKind.Object)
                    {
                        var objJson = valueProp.GetRawText();
                        var single = JsonSerializer.Deserialize<NewsDto>(objJson, _jsonOptions);
                        if (single != null) return single;
                    }
                    else if (valueProp.ValueKind == JsonValueKind.Array)
                    {
                        var arr = valueProp.EnumerateArray();
                        var first = arr.FirstOrDefault();
                        if (first.ValueKind == JsonValueKind.Object)
                        {
                            var objJson = first.GetRawText();
                            var single = JsonSerializer.Deserialize<NewsDto>(objJson, _jsonOptions);
                            if (single != null) return single;
                        }
                    }
                }

                // Some APIs wrap under "d" (WCF/older services)
                if (root.TryGetProperty("d", out var dProp))
                {
                    if (dProp.ValueKind == JsonValueKind.Object && dProp.TryGetProperty("results", out var results))
                    {
                        if (results.ValueKind == JsonValueKind.Array)
                        {
                            var first = results.EnumerateArray().FirstOrDefault();
                            if (first.ValueKind == JsonValueKind.Object)
                            {
                                var objJson = first.GetRawText();
                                var single = JsonSerializer.Deserialize<NewsDto>(objJson, _jsonOptions);
                                if (single != null) return single;
                            }
                        }
                        else if (results.ValueKind == JsonValueKind.Object)
                        {
                            var single = JsonSerializer.Deserialize<NewsDto>(results.GetRawText(), _jsonOptions);
                            if (single != null) return single;
                        }
                    }
                    else if (dProp.ValueKind == JsonValueKind.Object)
                    {
                        var single = JsonSerializer.Deserialize<NewsDto>(dProp.GetRawText(), _jsonOptions);
                        if (single != null) return single;
                    }
                }

                // Fallback: search for first JSON object that contains "NewsArticleId"
                foreach (var prop in root.EnumerateObject())
                {
                    if (prop.Value.ValueKind == JsonValueKind.Object)
                    {
                        var obj = prop.Value;
                        if (obj.TryGetProperty("NewsArticleId", out _))
                        {
                            var single = JsonSerializer.Deserialize<NewsDto>(obj.GetRawText(), _jsonOptions);
                            if (single != null) return single;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("TryParseNewsFromJson exception: " + ex);
            }

            return null;
        }

        private T? TryParseSingle<T>(string json)
        {
            // Try direct
            try
            {
                var direct = JsonSerializer.Deserialize<T>(json, _jsonOptions);
                if (direct != null) return direct;
            }
            catch { }

            // Try OData "value"
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (root.TryGetProperty("value", out var valueProp))
                {
                    if (valueProp.ValueKind == JsonValueKind.Object)
                    {
                        return JsonSerializer.Deserialize<T>(valueProp.GetRawText(), _jsonOptions);
                    }
                    else if (valueProp.ValueKind == JsonValueKind.Array)
                    {
                        var first = valueProp.EnumerateArray().FirstOrDefault();
                        if (first.ValueKind == JsonValueKind.Object)
                        {
                            return JsonSerializer.Deserialize<T>(first.GetRawText(), _jsonOptions);
                        }
                    }
                }
            }
            catch { }

            return default;
        }

        private List<T>? TryParseListFromOData<T>(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.ValueKind == JsonValueKind.Array)
                {
                    return JsonSerializer.Deserialize<List<T>>(json, _jsonOptions);
                }

                if (root.TryGetProperty("value", out var valueProp) && valueProp.ValueKind == JsonValueKind.Array)
                {
                    return JsonSerializer.Deserialize<List<T>>(valueProp.GetRawText(), _jsonOptions);
                }

                // older payloads may use "d.results"
                if (root.TryGetProperty("d", out var dProp) && dProp.ValueKind == JsonValueKind.Object && dProp.TryGetProperty("results", out var results) && results.ValueKind == JsonValueKind.Array)
                {
                    return JsonSerializer.Deserialize<List<T>>(results.GetRawText(), _jsonOptions);
                }

                // fallback: try to deserialize root as list
                return JsonSerializer.Deserialize<List<T>>(json, _jsonOptions);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("TryParseListFromOData exception: " + ex);
                return null;
            }
        }
    }
}
