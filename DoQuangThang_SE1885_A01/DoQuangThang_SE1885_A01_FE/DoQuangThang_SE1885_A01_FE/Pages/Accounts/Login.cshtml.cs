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

            var json = JsonSerializer.Serialize(new
            {
                email = Login.Email,
                password = Login.Password
            });

            var response = await client.PostAsJsonAsync(
             "api/auth/login",
            new
            {
            email = Login.Email,
            password = Login.Password
            });


            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                ErrorMessage = "Invalid email or password";
                return Page();
            }

            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = "Login failed";
                return Page();
            }

            var responseJson = await response.Content.ReadAsStringAsync();

            var loginResult = JsonSerializer.Deserialize<LoginResponse>(
                responseJson,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            HttpContext.Session.SetInt32("AccountId", loginResult!.AccountId);
            HttpContext.Session.SetInt32("Role", loginResult.Role);
            HttpContext.Session.SetString("Email", loginResult.Email);
            switch (loginResult!.Role)
            {
                case 0:
                    return RedirectToPage("/Reports/Index");
                case 1:
                    return RedirectToPage("/News/Index");
                default:
                    return RedirectToPage("/Index");
            }
        }
    }
}
