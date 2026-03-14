using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DoQuangThang_SE1885_A01_FE.Pages.Accounts
{
    public class LogoutModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public LogoutModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> OnGetAsync() 
        {
            var refreshToken = HttpContext.Session.GetString("RefreshToken");

            // 2. Nếu có token, gọi API để Revoke trên server
            if (!string.IsNullOrEmpty(refreshToken))
            {
                try
                {
                    // Tạo Client
                    var client = _httpClientFactory.CreateClient();

                    // Tạo payload giống LogoutRequest bên API
                    var logoutRequest = new { RefreshToken = refreshToken };
                  
                    var apiUrl = "https://localhost:7066/api/auth/logout";

                    var response = await client.PostAsJsonAsync(apiUrl, logoutRequest);


                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Logout API failed: {response.StatusCode}");
                        return RedirectToPage("/Accounts/Login");
                    }

                    HttpContext.Session.Clear();

                    return RedirectToPage("/Index");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error calling Logout API: {ex.Message}");
                    HttpContext.Session.Clear();

                    return RedirectToPage("/Index");
                }
            }

            return RedirectToPage("/Error");
        }
    }
}
