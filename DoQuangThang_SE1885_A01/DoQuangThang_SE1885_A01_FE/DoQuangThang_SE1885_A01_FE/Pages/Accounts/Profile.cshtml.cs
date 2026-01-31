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

        // Keep these as properties so the Razor view can render values,
        // but do NOT use [BindProperty] to avoid automatic cross-form validation.
        public UpdateProfileInputModel ProfileInput { get; set; }
        public ChangePasswordInputModel PasswordInput { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadAccountProfile();
            if (CurrentAccount == null) return RedirectToPage("/Index");
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateProfileAsync(UpdateProfileInputModel profileInput)
        {
            // Validate only the incoming model using the same prefix used in the form (ProfileInput)
            if (!TryValidateModel(profileInput, "ProfileInput"))
            {
                ProfileInput = profileInput;
                await LoadAccountProfile();
                return Page();
            }

            var accountId = HttpContext.Session.GetInt32("AccountId");
            if (accountId == null) return RedirectToPage("/Index");

            var client = _httpClientFactory.CreateClient("NewsAPI");
            string requestUrl = $"api/account({accountId})";

            var payload = new
            {
                AccountName = profileInput.AccountName,
                AccountEmail = profileInput.AccountEmail
            };

            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync(requestUrl, content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToPage(); // PRG
            }

            var errorMsg = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, errorMsg);
            TempData["ErrorMessage"] = "Failed to update profile. " + errorMsg;

            ProfileInput = profileInput;
            await LoadAccountProfile();
            return Page();
        }

        public async Task<IActionResult> OnPostChangePasswordAsync(ChangePasswordInputModel passwordInput)
        {
            // Validate only the incoming model using the same prefix used in the form (PasswordInput)
            if (!TryValidateModel(passwordInput, "PasswordInput"))
            {
                PasswordInput = passwordInput;
                await LoadAccountProfile();
                return Page();
            }

            var accountId = HttpContext.Session.GetInt32("AccountId");
            if (accountId == null) return RedirectToPage("/Index");

            var client = _httpClientFactory.CreateClient("NewsAPI");
            string requestUrl = $"api/auth/change-password?accountId={accountId}&oldPassword={passwordInput.OldPassword}&newPassword={passwordInput.NewPassword}";

            // HttpPatch expects a body, send empty JSON
            var content = new StringContent("{}", Encoding.UTF8, "application/json");
            var response = await client.PatchAsync(requestUrl, content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Password changed successfully!";
                return RedirectToPage(); // PRG to clear form
            }

            var errorMsg = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, errorMsg);
            TempData["ErrorMessage"] = "Failed to change password. " + errorMsg;

            PasswordInput = passwordInput;
            await LoadAccountProfile();
            return Page();
        }

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

                    // Initialize editable model only if not already set (preserve submitted values on failed POST)
                    if (ProfileInput == null)
                    {
                        ProfileInput = new UpdateProfileInputModel
                        {
                            AccountName = CurrentAccount?.AccountName,
                            AccountEmail = CurrentAccount?.AccountEmail
                        };
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
        public int AccountRole { get; set; }
        public string AccountPassword { get; set; }
    }

    public class UpdateProfileInputModel
    {
        [Required(ErrorMessage = "Full name is required")]
        public string AccountName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string AccountEmail { get; set; }
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
}