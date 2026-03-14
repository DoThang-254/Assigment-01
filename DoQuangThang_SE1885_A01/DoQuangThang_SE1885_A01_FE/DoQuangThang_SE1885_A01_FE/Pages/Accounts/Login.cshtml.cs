using DoQuangThang_SE1885_A01_FE.Models.Accounts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;

namespace DoQuangThang_SE1885_A01_FE.Pages.Auth
{
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        [BindProperty]
        public LoginDto Login { get; set; } = new();

        public string? ErrorMessage { get; set; }
        public LoginModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var client = _httpClientFactory.CreateClient("NewsAPI");

            // 1. Gửi request (Code gọn hơn)
            var response = await client.PostAsJsonAsync("api/auth/login", new
            {
                email = Login.Email,
                password = Login.Password
            });

            // 2. Xử lý lỗi 401 riêng
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                ErrorMessage = "Email or Password is not valid";
                return Page();
            }

            // 3. Xử lý các lỗi khác
            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = "Failed to Sign In!";
                return Page();
            }

            // 4. Đọc dữ liệu trả về
            var responseJson = await response.Content.ReadAsStringAsync();
            var loginResult = JsonSerializer.Deserialize<LoginResponse>(
                responseJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            if (loginResult == null)
            {
                ErrorMessage = "Something went wrong";
                return Page();
            }


            HttpContext.Session.SetString("AccessToken", loginResult.AccessToken);
            if (!string.IsNullOrEmpty(loginResult.RefreshToken))
            {
                HttpContext.Session.SetString("RefreshToken", loginResult.RefreshToken);
            }

            // Lưu các thông tin phụ trợ
            HttpContext.Session.SetInt32("AccountId", loginResult.AccountId);
            HttpContext.Session.SetString("Email", loginResult.Email ?? ""); 

            int roleValue = loginResult.Role ?? -1;
            HttpContext.Session.SetInt32("Role", roleValue);

            return roleValue switch
            {
                0 => RedirectToPage("/Reports/Analytics"), // Admin
                1 => RedirectToPage("/News/Index"),    // Staff
                2 => RedirectToPage("/Lecturer/Index"),// Lecturer
                _ => RedirectToPage("/Index")          // Khách
            };
        }
    }
}
