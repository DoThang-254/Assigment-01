using DoQuangThang_SE1885_A01_FE.Models.Accounts;
using DoQuangThang_SE1885_A01_FE.Models.Categories;
using DoQuangThang_SE1885_A01_FE.Models.News;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using System.Text.Json;

namespace DoQuangThang_SE1885_A01_FE.Pages.News 
{
    public class DetailModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly IMemoryCache _cache; 

        public DetailModel(IHttpClientFactory httpClientFactory, IMemoryCache cache)
        {
            _httpClientFactory = httpClientFactory;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            _cache = cache;
        }

        public NewsDto News { get; set; }

        public List<NewsDto> RelatedArticles { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var client = _httpClientFactory.CreateClient("NewsAPI");

            // =====================================================
            // 1. LẤY BÀI VIẾT CHÍNH (GỌI TRỰC TIẾP API - KHÔNG CACHE)
            // =====================================================
            var response = await client.GetAsync($"api/news('{id}')?$expand=Category,CreatedBy,Tags");
            if (!response.IsSuccessStatusCode) return NotFound();

            var json = await response.Content.ReadAsStringAsync();
            Debug.WriteLine("News detail json: " + json);

            News = TryParseNewsFromJson(json);

            // Xử lý Fallback nếu Expand bị lỗi (Giữ nguyên logic cực kỳ cẩn thận của bạn)
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
                    catch (Exception ex) { Debug.WriteLine("Category fetch failed: " + ex); }
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
                    catch (Exception ex) { Debug.WriteLine("CreatedBy fetch failed: " + ex); }
                }
            }

            if (News == null)
            {
                Debug.WriteLine("Failed to deserialize News detail or expanded properties missing.");
                return Page();
            }

            // =====================================================
            // 2. LẤY BÀI VIẾT LIÊN QUAN (TỐI ƯU BẰNG RAM CACHE 30 PHÚT)
            // =====================================================
            string relatedCacheKey = $"RelatedNews_For_Article_{id}";

            // Kiểm tra xem đã có danh sách gợi ý cho bài viết này trong RAM chưa?
            if (!_cache.TryGetValue(relatedCacheKey, out List<NewsDto> cachedRelated))
            {
                // Nếu chưa có, tiến hành build Query OData chọc xuống DB
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
                    cachedRelated = TryParseListFromOData<NewsDto>(relatedJson) ?? new List<NewsDto>();

                    // Cất vào bộ nhớ đệm (RAM) trong 30 phút
                    _cache.Set(relatedCacheKey, cachedRelated, TimeSpan.FromMinutes(30));
                }
            }

            // Gán dữ liệu (từ Cache hoặc API) ra giao diện
            RelatedArticles = cachedRelated ?? new List<NewsDto>();

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
