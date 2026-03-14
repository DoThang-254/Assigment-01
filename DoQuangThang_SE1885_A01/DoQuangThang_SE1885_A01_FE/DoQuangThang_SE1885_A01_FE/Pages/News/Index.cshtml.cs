using DoQuangThang_SE1885_A01_FE.Hubs;
using DoQuangThang_SE1885_A01_FE.Models;
using DoQuangThang_SE1885_A01_FE.Models.Accounts;
using DoQuangThang_SE1885_A01_FE.Models.Categories;
using DoQuangThang_SE1885_A01_FE.Models.News;
using DoQuangThang_SE1885_A01_FE.Models.Tags;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace DoQuangThang_SE1885_A01_FE.Pages.News
{
    public class IndexModel : StaffAuthorize
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly IHubContext<ReportHub> _reportHub;
        private readonly IMemoryCache _memoryCache; 

       public IndexModel(IHttpClientFactory httpClientFactory, IHubContext<ReportHub> reportHub, IMemoryCache memoryCache)
        {
            _httpClientFactory = httpClientFactory;
            _reportHub = reportHub;
            _memoryCache = memoryCache;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        // --- Data Properties ---
        public List<NewsDto> NewsList { get; set; } = new();
        public List<CategoryDto> Categories { get; set; } = new(); // Để đổ vào Dropdown Modal

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

        public async Task OnGetAsync()
        {
            // Reset thông báo lỗi cũ
            TempData["ErrorMessage"] = null;

            try
            {
                var client = _httpClientFactory.CreateClient("NewsAPI");
                if (!_memoryCache.TryGetValue("/api/category", out string categoryJson))
                {
                    // Nếu không có trong RAM, mới gọi API
                    var categoryResponse = await client.GetAsync("api/category");
                    if (categoryResponse.IsSuccessStatusCode)
                    {
                        categoryJson = await categoryResponse.Content.ReadAsStringAsync();
                        // Lưu vào RAM ngay để lần sau dùng
                        _memoryCache.Set("/api/category", categoryJson, TimeSpan.FromMinutes(10));
                    }
                }
                // Parse JSON (từ Cache hoặc từ API) ra Object
                if (!string.IsNullOrEmpty(categoryJson))
                {
                    var odataCategories = JsonSerializer.Deserialize<ODataResponse<CategoryDto>>(categoryJson, _jsonOptions);
                    Categories = odataCategories?.Value ?? new List<CategoryDto>();
                }

                // ==========================================
                // 2. LẤY TAG TỪ RAM CACHE (Nếu có)
                // ==========================================
                // ==========================================
                // 2. LẤY TAG TỪ RAM CACHE (Hoặc API)
                // ==========================================
                if (!_memoryCache.TryGetValue("/api/tag", out string tagJson))
                {
                    Console.WriteLine("🌐 TAG DATA: Calling API...");
                    var tagResponse = await client.GetAsync("api/tag");
                    if (tagResponse.IsSuccessStatusCode)
                    {
                        tagJson = await tagResponse.Content.ReadAsStringAsync();
                        _memoryCache.Set("/api/tag", tagJson, TimeSpan.FromMinutes(10));
                        Console.WriteLine("💾 TAG DATA: Saved to MEMORY CACHE");
                    }
                }

                // THÊM ĐOẠN NÀY: Parse JSON ra list AllTags để FE có data hiển thị
                if (!string.IsNullOrEmpty(tagJson))
                {
                    var odataTags = JsonSerializer.Deserialize<ODataResponse<TagDto>>(tagJson, _jsonOptions);
                    AllTags = odataTags?.Value ?? new List<TagDto>();
                    Console.WriteLine($"🟢 TAG DATA: Loaded {AllTags.Count} tags to AllTags property.");
                }


                // ==========================================
                // 3. XÂY DỰNG QUERY TIN TỨC (Giữ nguyên logic tạo filter của bạn)
                // ==========================================
                var query = new StringBuilder("api/news?$expand=Category,CreatedBy,Tags&$orderby=CreatedDate desc&$count=true");
                var filters = new List<string>();

                if (!string.IsNullOrEmpty(Keyword))
                {
                    string k = Keyword.Trim().Replace("'", "''");
                    filters.Add($"(contains(NewsTitle, '{k}') or contains(Category/CategoryName, '{k}') or contains(CreatedBy/AccountName, '{k}'))");
                }

                if (!string.IsNullOrEmpty(Status) && bool.TryParse(Status, out bool statusVal))
                {
                    filters.Add($"NewsStatus eq {statusVal.ToString().ToLower()}");
                }

                if (StartDate.HasValue)
                    filters.Add($"CreatedDate ge {StartDate.Value:yyyy-MM-ddTHH:mm:ss}Z");

                if (EndDate.HasValue)
                    filters.Add($"CreatedDate le {EndDate.Value.AddDays(1):yyyy-MM-ddTHH:mm:ss}Z");

                if (filters.Any())
                    query.Append("&$filter=" + string.Join(" and ", filters));

                if (CurrentPage < 1) CurrentPage = 1;
                int skip = (CurrentPage - 1) * PageSize;
                query.Append($"&$skip={skip}&$top={PageSize}");

                string finalQueryUrl = query.ToString();

                // ==========================================
                // 4. GỌI THẲNG API (Đã xóa IMemoryCache ở đây)
                // ==========================================
                var response = await client.GetAsync(finalQueryUrl); // Luôn luôn gọi API để lấy data mới nhất

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var odataResult = JsonSerializer.Deserialize<ODataResponse<NewsDto>>(json, _jsonOptions);

                    NewsList = odataResult?.Value ?? new();
                    if (odataResult != null)
                    {
                        TotalItems = odataResult.Count;
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = $"Failed to load news. Server returned: {response.StatusCode}";
                }
            }
            catch (HttpRequestException ex)
            {
                // Đây là nơi bắt lỗi nếu Polly đã thử lại 3 lần mà VẪN MẤT MẠNG hoặc Server CHẾT hẳn
                TempData["ErrorMessage"] = "Network unstable. Retried connection but failed. Please check your internet.";

                // Vẫn khởi tạo list rỗng để tránh lỗi NullReference ở View
                NewsList = new List<NewsDto>();
                Categories = new List<CategoryDto>();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred: " + ex.Message;
                NewsList = new List<NewsDto>();
            }
        }

        //public async Task OnGetAsync()
        //{
        //    // Reset thông báo lỗi cũ
        //    TempData["ErrorMessage"] = null;

        //    try
        //    {
        //        var client = _httpClientFactory.CreateClient("NewsAPI");

        //        // 1. Tối ưu: Gọi song song Tag và Category
        //        // Polly sẽ tự động Retry cho từng request này nếu lỗi
        //        var tagTask = client.GetAsync("api/tag");
        //        var categoryTask = client.GetAsync("api/category");

        //        // Chờ cả 2 xong
        //        await Task.WhenAll(tagTask, categoryTask);

        //        var tagResponse = tagTask.Result;
        //        var categoryResponse = categoryTask.Result;

        //        // Xử lý Category
        //        if (categoryResponse.IsSuccessStatusCode)
        //        {
        //            var categoryJson = await categoryResponse.Content.ReadAsStringAsync();
        //            var odataCategories = JsonSerializer.Deserialize<ODataResponse<CategoryDto>>(
        //                categoryJson, _jsonOptions);
        //            Categories = odataCategories?.Value ?? new List<CategoryDto>();
        //        }
        //        else
        //        {
        //            // Nếu API trả về lỗi logic (vd 404, 400 - Polly mặc định không retry cái này)
        //            Categories = new List<CategoryDto>();
        //        }

        //        // Xử lý Tag
        //        if (tagResponse.IsSuccessStatusCode)
        //        {
        //            var tagJson = await tagResponse.Content.ReadAsStringAsync();
        //            var odataResult = JsonSerializer.Deserialize<ODataResponse<TagDto>>(
        //                tagJson, _jsonOptions);
        //            AllTags = odataResult?.Value ?? new List<TagDto>();
        //        }

        //        // 2. Xây dựng Query cho News
        //        var query = new StringBuilder("api/news?$expand=Category,CreatedBy,Tags&$orderby=CreatedDate desc&$count=true");
        //        var filters = new List<string>();

        //        if (!string.IsNullOrEmpty(Keyword))
        //        {
        //            string k = Keyword.Trim().Replace("'", "''"); // Chống OData Injection
        //            filters.Add($"(contains(NewsTitle, '{k}') or contains(Category/CategoryName, '{k}') or contains(CreatedBy/AccountName, '{k}'))");
        //        }

        //        if (!string.IsNullOrEmpty(Status) && bool.TryParse(Status, out bool statusVal))
        //        {
        //            filters.Add($"NewsStatus eq {statusVal.ToString().ToLower()}");
        //        }

        //        if (StartDate.HasValue)
        //            filters.Add($"CreatedDate ge {StartDate.Value:yyyy-MM-ddTHH:mm:ss}Z");

        //        if (EndDate.HasValue)
        //            filters.Add($"CreatedDate le {EndDate.Value.AddDays(1):yyyy-MM-ddTHH:mm:ss}Z");

        //        if (filters.Any())
        //            query.Append("&$filter=" + string.Join(" and ", filters));

        //        if (CurrentPage < 1) CurrentPage = 1;
        //        int skip = (CurrentPage - 1) * PageSize;
        //        query.Append($"&$skip={skip}&$top={PageSize}");

        //        // 3. Gọi API News (Polly cũng sẽ retry cái này nếu mạng chập chờn)
        //        var response = await client.GetAsync(query.ToString());

        //        if (response.IsSuccessStatusCode)
        //        {
        //            var json = await response.Content.ReadAsStringAsync();
        //            var odataResult = JsonSerializer.Deserialize<ODataResponse<NewsDto>>(json, _jsonOptions);
        //            NewsList = odataResult?.Value ?? new();
        //            if (odataResult != null)
        //            {
        //                TotalItems = odataResult.Count;
        //            }
        //        }
        //        else
        //        {
        //            // API phản hồi nhưng báo lỗi (vd: 500 Server Error mà Polly đã retry hết số lần)
        //            TempData["ErrorMessage"] = $"Failed to load news. Server returned: {response.StatusCode}";
        //        }
        //    }
        //    catch (HttpRequestException ex)
        //    {
        //        // Đây là nơi bắt lỗi nếu Polly đã thử lại 3 lần mà VẪN MẤT MẠNG hoặc Server CHẾT hẳn
        //        TempData["ErrorMessage"] = "Network unstable. Retried connection but failed. Please check your internet.";

        //        // Vẫn khởi tạo list rỗng để tránh lỗi NullReference ở View
        //        NewsList = new List<NewsDto>();
        //        Categories = new List<CategoryDto>();
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["ErrorMessage"] = "An unexpected error occurred: " + ex.Message;
        //        NewsList = new List<NewsDto>();
        //    }
        //}


        /// --- AI Tag Suggestion ---
        public async Task<IActionResult> OnPostSuggestTagsAsync([FromBody] string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return new JsonResult(new List<string>());

            // SỬA: Dùng đúng client "AIAPI" đã cấu hình trong Program.cs
            var client = _httpClientFactory.CreateClient("AIAPI");

            var payload = new { Content = content };
            var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            // Route: api/AI/suggest-tags (Đảm bảo bên AI API Controller cũng map đúng route này)
            var response = await client.PostAsync("api/ai/suggest-tags", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                var jsonResult = await response.Content.ReadAsStringAsync();
                var tags = JsonSerializer.Deserialize<List<string>>(jsonResult, _jsonOptions);
                return new JsonResult(tags);
            }

            return new JsonResult(new List<string>());
        }



        [BindProperty(SupportsGet = true)]
        public List<TagDto> AllTags { get; set; }

        [BindProperty]
        public List<int> TagIds { get; set; }

        [BindProperty]
        public IFormFile? UploadFile { get; set; }

        public async Task<IActionResult> OnPostSaveAsync(NewsDto news)
        {
            var currentAccountId = HttpContext.Session.GetInt32("AccountId");

            if (!currentAccountId.HasValue)
            {
                TempData["ErrorMessage"] = "You must be logged in to create news.";
                return RedirectToPage("/Index");
            }

            // --- DEBUG: In ra giá trị ImageUrl được binding từ form (nếu có thẻ ẩn thì nó sẽ hiện ở đây)
            Console.WriteLine($"[DEBUG-1] Bắt đầu Save. isUpdate: {!string.IsNullOrEmpty(news.NewsArticleId)}, ImageUrl từ Form: {news.ImageUrl ?? "NULL"}");

            // 1. Xử lý các giá trị mặc định tránh Null
            if (string.IsNullOrEmpty(news.NewsSource))
            {
                news.NewsSource = "System";
            }

            // 2. Gán ID người tạo/sửa
            short accountId = (short)currentAccountId.Value;
            news.CreatedById = accountId;
            news.UpdatedById = accountId;

            var client = _httpClientFactory.CreateClient("NewsAPI");

            if (UploadFile != null && UploadFile.Length > 0)
            {
                Console.WriteLine($"[DEBUG-2] Có UploadFile mới: {UploadFile.FileName}, size: {UploadFile.Length}");
                using var content = new MultipartFormDataContent();

                var fileStream = UploadFile.OpenReadStream();
                var streamContent = new StreamContent(fileStream);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(UploadFile.ContentType);
                content.Add(streamContent, "file", UploadFile.FileName);

                var uploadResponse = await client.PostAsync("api/news/UploadImage", content);

                if (uploadResponse.IsSuccessStatusCode)
                {
                    var uploadResult = await uploadResponse.Content.ReadAsStringAsync();
                    using var jsonDoc = JsonDocument.Parse(uploadResult);
                    string imageUrl = jsonDoc.RootElement.GetProperty("url").GetString();
                    news.ImageUrl = imageUrl;

                    Console.WriteLine($"[DEBUG-3] Upload ảnh thành công. ImageUrl mới: {news.ImageUrl}");
                }
                else
                {
                    Console.WriteLine($"[DEBUG-3-ERROR] Upload ảnh thất bại. Status: {uploadResponse.StatusCode}");
                    TempData["ErrorMessage"] = "Failed to upload image.";
                    return RedirectToPage();
                }
            }
            else
            {
                Console.WriteLine("[DEBUG-2] KHÔNG CÓ UploadFile mới được chọn.");
            }

            bool isUpdate = !string.IsNullOrEmpty(news.NewsArticleId) && news.NewsArticleId != "0";

            // NẾU LÀ UPDATE VÀ KHÔNG CHỌN ẢNH MỚI -> Lấy lại ảnh cũ
            if (isUpdate && (UploadFile == null || UploadFile.Length == 0))
            {
                string getOldNewsUrl = $"api/news/{news.NewsArticleId}";
                Console.WriteLine($"[DEBUG-4] Đang gọi API lấy bài viết cũ để giữ ảnh. URL: {getOldNewsUrl}");

                var oldNewsResponse = await client.GetAsync(getOldNewsUrl);

                if (oldNewsResponse.IsSuccessStatusCode)
                {
                    var oldNewsJson = await oldNewsResponse.Content.ReadAsStringAsync();
                    var oldNews = JsonSerializer.Deserialize<NewsDto>(oldNewsJson, _jsonOptions);

                    Console.WriteLine($"[DEBUG-5] Lấy bài viết cũ THÀNH CÔNG. ImageUrl cũ trên API là: {oldNews?.ImageUrl ?? "NULL"}");

                    if (oldNews != null && !string.IsNullOrEmpty(oldNews.ImageUrl))
                    {
                        news.ImageUrl = oldNews.ImageUrl;
                    }
                }
                else
                {
                    Console.WriteLine($"[DEBUG-5-ERROR] Lấy bài viết cũ THẤT BẠI. Status Code: {oldNewsResponse.StatusCode}. Message: {await oldNewsResponse.Content.ReadAsStringAsync()}");
                    // Rất có thể URL "api/news/{id}" của bạn bị sai route, dẫn đến 404 Not Found ở đây!
                }
            }

            var userSelectedTagIds = this.TagIds ?? new List<int>();

            var payload = new
            {
                NewsArticleId = news.NewsArticleId,
                NewsTitle = news.NewsTitle,
                Headline = news.Headline,
                NewsContent = news.NewsContent,
                NewsSource = news.NewsSource,
                CategoryId = news.CategoryId,
                NewsStatus = news.NewsStatus,
                CreatedById = news.CreatedById,
                UpdatedById = news.UpdatedById,
                CreatedDate = news.CreatedDate ?? DateTime.Now,
                TagIds = this.TagIds ?? new List<int>(),
                ImageUrl = news.ImageUrl
            };

            var jsonContentString = JsonSerializer.Serialize(payload);
            Console.WriteLine($"[DEBUG-6] Payload chuẩn bị gửi đi:\n{jsonContentString}");

            var jsonContent = new StringContent(jsonContentString, Encoding.UTF8, "application/json");

            HttpResponseMessage response;

            if (isUpdate)
            {
                Console.WriteLine("[DEBUG-7] Gửi request PATCH để Update...");
                var request = new HttpRequestMessage(HttpMethod.Patch, "api/news")
                {
                    Content = jsonContent
                };
                response = await client.SendAsync(request);
            }
            else
            {
                Console.WriteLine("[DEBUG-7] Gửi request POST để Create...");
                response = await client.PostAsync("api/news", jsonContent);
            }

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("[DEBUG-8] Lưu/Update THÀNH CÔNG vào Database!");
                // ... (phần code Suggest Tags giữ nguyên không sửa) ...

                TempData["SuccessMessage"] = isUpdate ? "News updated successfully!" : "News created successfully!";
                await _reportHub.Clients.All.SendAsync("ReportsUpdated");
            }
            else
            {
                var err = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[DEBUG-8-ERROR] Lưu/Update THẤT BẠI. Status: {response.StatusCode}. Error: {err}");
                TempData["ErrorMessage"] = $"Action failed: {err}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            var client = _httpClientFactory.CreateClient("NewsAPI");

            var response = await client.DeleteAsync($"api/news('{id}')");

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "News deleted successfully!";
                await _reportHub.Clients.All.SendAsync("ReportsDeleted");
            }
            else
            {
                TempData["ErrorMessage"] = "Delete failed.";
            }

            return RedirectToPage();
        }
    }
}