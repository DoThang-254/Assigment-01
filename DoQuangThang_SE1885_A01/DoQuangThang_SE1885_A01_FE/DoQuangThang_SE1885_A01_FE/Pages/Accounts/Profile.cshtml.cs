using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;

namespace DoQuangThang_SE1885_A01_FE.Pages.Accounts
{
    public class ProfileModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _jsonOptions;

        public ProfileModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public SystemAccountDto CurrentAccount { get; set; }
        [BindProperty]
        public ChangePasswordInputModel PasswordInput { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadAccountProfile();
            if (CurrentAccount == null) return RedirectToPage("/Index");
            return Page();
        }

        // Hàm xử lý khi người dùng bấm nút Change Password
        public async Task<IActionResult> OnPostChangePasswordAsync()
        {
            // 1. Validate Form
            if (!ModelState.IsValid)
            {
                await LoadAccountProfile(); // Load lại thông tin user để hiển thị lại trang
                return Page();
            }

            var accountId = HttpContext.Session.GetInt32("AccountId");
            if (accountId == null) return RedirectToPage("/Index");

            // 2. Gọi API Patch
            var client = _httpClientFactory.CreateClient("NewsAPI");

            // Vì Controller của bạn nhận tham số qua Query String hoặc Form, 
            // với HttpPatch và signature (int, string, string), cách an toàn nhất để gọi là truyền qua Query String 
            // (hoặc phải sửa Controller để nhận Object [FromBody]).
            // Dưới đây là cách gọi theo Signature Controller bạn cung cấp:

            string requestUrl = $"api/auth/change-password?accountId={accountId}&oldPassword={PasswordInput.OldPassword}&newPassword={PasswordInput.NewPassword}";

            // HttpPatch yêu cầu một body, dù rỗng
            var content = new StringContent("", Encoding.UTF8, "application/json");

            var response = await client.PatchAsync(requestUrl, content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Password changed successfully!";
                // Reset form
                PasswordInput = new ChangePasswordInputModel();
            }
            else
            {
                // Đọc lỗi từ API trả về (BadRequest)
                var errorMsg = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, errorMsg);
                TempData["ErrorMessage"] = "Failed to change password. " + errorMsg;
            }

            await LoadAccountProfile();
            return Page();
        }

        // Tách logic load profile ra hàm riêng để tái sử dụng
        private async Task LoadAccountProfile()
        {
            var accountId = HttpContext.Session.GetInt32("AccountId");
            if (accountId != null)
            {
                var client = _httpClientFactory.CreateClient("NewsAPI");
                var response = await client.GetAsync($"api/account({accountId})");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    CurrentAccount = JsonSerializer.Deserialize<SystemAccountDto>(json, _jsonOptions);
                }
            }
        }
    }

}

public class SystemAccountDto
{
    public short AccountId { get; set; }
    public string AccountName { get; set; }
    public string AccountEmail { get; set; }
    public int AccountRole { get; set; } // 1: Staff, 2: Lecturer, etc.
    public string AccountPassword { get; set; } // Thường không nên hiển thị, nhưng tôi sẽ để vào theo yêu cầu
}

public class ChangePasswordInputModel
{
    [Required(ErrorMessage = "Old password is required")]
    [DataType(DataType.Password)]
    public string OldPassword { get; set; }

    [Required(ErrorMessage = "New password is required")]
    [DataType(DataType.Password)]
    [MinLength(3, ErrorMessage = "Password must be at least 3 characters")]
    public string NewPassword { get; set; }

    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; }
}