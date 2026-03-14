using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AIAPI.Controllers
{
    [Route("api/[controller]")]
    public class AIController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        private static ConcurrentDictionary<string, int> _learningCache = new();

        public AIController(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public class ContentRequest { public string Content { get; set; } }
        public class LearningRequest { public List<string> SelectedTags { get; set; } }

        [HttpPost("suggest-tags")]
        public async Task<IActionResult> SuggestTags([FromBody] ContentRequest request)
        {
            // LOG 1: Kiểm tra xem Request có vào được Controller không và nội dung là gì
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine($"[DEBUG] >>> API SuggestTags HIT. Content: '{request.Content}'");

            if (string.IsNullOrWhiteSpace(request.Content))
            {
                Console.WriteLine("[DEBUG] Content is null or empty. Returning empty list.");
                return Ok(new List<string>());
            }

            var suggestedTags = new List<string>();

            try
            {
                // LOG 2: Bắt đầu gọi OpenAI
                Console.WriteLine("[DEBUG] 1. Calling OpenAI API...");

                var openAiTags = await CallOpenAiForTags(request.Content);

                // LOG 3: Kết quả từ OpenAI
                if (openAiTags != null && openAiTags.Any())
                {
                    Console.WriteLine($"[DEBUG] OpenAI Success. Found {openAiTags.Count} tags: [{string.Join(", ", openAiTags)}]");
                    suggestedTags.AddRange(openAiTags);
                }
                else
                {
                    Console.WriteLine("[DEBUG] OpenAI returned NO tags (Empty list).");
                }
            }
            catch (Exception ex)
            {
                // LOG 4: Lỗi OpenAI (Quan trọng để biết tại sao API chết)
                Console.WriteLine($"[DEBUG] OpenAI Error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[DEBUG] Inner Exception: {ex.InnerException.Message}");
                }
            }

            // 2. KẾT HỢP VỚI LEARNING CACHE
            // LOG 5: Kiểm tra Cache hiện tại
            Console.WriteLine($"[DEBUG] 2. Checking Learning Cache. Total items in cache: {_learningCache.Count}");

            // In thử toàn bộ cache để xem có gì bên trong không
            foreach (var item in _learningCache)
            {
                Console.WriteLine($"[DEBUG] Cache Item: {item.Key} - Count: {item.Value}");
            }

            var learnedTags = _learningCache.OrderByDescending(x => x.Value)
                                            .Take(3)
                                            .Select(x => x.Key)
                                            .ToList();

            Console.WriteLine($"[DEBUG] Top 3 Cached Tags: [{string.Join(", ", learnedTags)}]");

            // 3. GỘP KẾT QUẢ
            var finalResult = suggestedTags.Union(learnedTags)
                                           .Distinct(StringComparer.OrdinalIgnoreCase)
                                           .Take(10)
                                           .ToList();

            // LOG 6: Kết quả cuối cùng trả về cho Frontend
            Console.WriteLine($"[DEBUG] <<< FINAL RESULT returning to Client: [{string.Join(", ", finalResult)}]");
            Console.WriteLine("--------------------------------------------------");

            return Ok(finalResult);
        }

        [HttpPost("learn")]
        public IActionResult LearnFromSelection([FromBody] LearningRequest request)
        {
            if (request.SelectedTags != null)
            {
                foreach (var tag in request.SelectedTags)
                {
                    _learningCache.AddOrUpdate(tag, 1, (key, oldValue) => oldValue + 1);
                }
            }
            return Ok(new { message = "AI knowledge updated" });
        }

        // --- HÀM PHỤ TRỢ: Xử lý gọi HTTP sang OpenAI ---
        private async Task<List<string>> CallOpenAiForTags(string content)
        {
            var apiKey = _configuration["OpenAI:ApiKey"];
            var model = _configuration["OpenAI:Model"];
            var baseUrl = _configuration["OpenAI:BaseUrl"];

            using var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            // 2. Prompt Engineering
            // 2. Prompt Engineering (ĐÃ SỬA LẠI ĐỂ GỢI Ý TAG THÔNG MINH HƠN)
            var systemPrompt = "You are an expert SEO specialist and content categorizer. Your task is to generate relevant, broad, and popular tags/categories for the given content.";

            var prompt = $"Read the following content and suggest 5 to 7 highly relevant tags (topics, themes, or categories). " +
                         $"The tags should be short (1-2 words), widely used, and DO NOT have to be exact words from the text. " +
                         $"Return ONLY a comma-separated list of tags. No explanations, no punctuation at the end. Content: \"{content}\"";

            var requestBody = new
            {
                model = model,
                messages = new[]
                {
        new { role = "system", content = systemPrompt },
        new { role = "user", content = prompt }
    },
                temperature = 0.7 // Tăng temperature lên một chút (0.7 thay vì 0.5) để AI sáng tạo ra các Tag phong phú hơn
            };

            // 3. Gọi API (Dùng baseUrl động)
            var response = await client.PostAsync(
                baseUrl, // <--- ĐÃ SỬA CHỖ NÀY
                new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json"));

            // 4. Xử lý lỗi (Thêm Log để debug nếu lỗi)
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[AI ERROR] Status: {response.StatusCode}. Details: {errorContent}");
                return new List<string>();
            }

            // 5. Parse kết quả
            var jsonResponse = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonResponse);

            try
            {
                var resultText = doc.RootElement
                                    .GetProperty("choices")[0]
                                    .GetProperty("message")
                                    .GetProperty("content")
                                    .GetString();

                if (!string.IsNullOrEmpty(resultText))
                {
                    // Sửa thêm: Xóa dấu chấm (.) ở cuối nếu AI lỡ thêm vào
                    return resultText.Replace(".", "")
                                     .Split(',')
                                     .Select(t => t.Trim())
                                     .Where(t => !string.IsNullOrEmpty(t))
                                     .Select(t => System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(t.ToLower()))
                                     .ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PARSE ERROR] Could not parse AI response: {ex.Message}");
            }

            return new List<string>();
        }
    }
}

