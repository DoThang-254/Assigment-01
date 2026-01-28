using DoQuangThang_SE1885_A01_FE.Models;
using DoQuangThang_SE1885_A01_FE.Models.Accounts;
using DoQuangThang_SE1885_A01_FE.Pages.Accounts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json; // Cần thêm namespace này cho PostAsJsonAsync
using System.Text.Json;

namespace DoQuangThang_SE1885_A01_FE.Pages.Auth
{
    public class IndexModel : AdminAuthorizeModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration; 

        public IndexModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [BindProperty]
        public List<AccountsDto> Accounts { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Keyword { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? Role { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageIndex { get; set; } = 1;

        public int PageSize { get; set; } = 3;
        public int TotalCount { get; set; }

        public async Task OnGetAsync()
        {
            await LoadData();
        }

        private async Task LoadData()
        {
            var client = _httpClientFactory.CreateClient("NewsAPI");
            int skip = (PageIndex - 1) * PageSize;

            var query = new List<string>
            {
                $"$top={PageSize}",
                $"$skip={skip}",
                "$count=true"
            };

            var filters = new List<string>();
            if (!string.IsNullOrWhiteSpace(Keyword)) filters.Add(BuildKeywordFilter(Keyword));
            if (Role.HasValue) filters.Add($"AccountRole eq {Role.Value}");

            if (filters.Any())
            {
                query.Add($"$filter={Uri.EscapeDataString(string.Join(" and ", filters))}");
            }

            string url = "api/account?" + string.Join("&", query);
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var odata = JsonSerializer.Deserialize<ODataResponse<AccountsDto>>(
                    json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                Accounts = odata?.Value ?? new();
                TotalCount = odata?.Count ?? 0;
            }
        }

        [BindProperty]
        public AccountInputModel AccountInput { get; set; } 

        public async Task<IActionResult> OnPostSaveAccountAsync()
        {
            var client = _httpClientFactory.CreateClient("NewsAPI");

            if (AccountInput.AccountId == 0)
            {
                var createRequest = new
                {
                    AccountName = AccountInput.AccountName,
                    AccountEmail = AccountInput.AccountEmail,
                    AccountRole = AccountInput.AccountRole,
                    AccountPassword = AccountInput.AccountPassword 
                };

                var response = await client.PostAsJsonAsync("api/account", createRequest);
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Account created successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to create account. " + await response.Content.ReadAsStringAsync();
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(AccountInput.AccountPassword))
                {
                    string adminEmail = _configuration["AdminAccount:Email"];
                    string adminPassword = _configuration["AdminAccount:Password"];

                    if (string.IsNullOrEmpty(AccountInput.AdminVerifyPassword))
                    {
                        TempData["ErrorMessage"] = "You must enter YOUR Admin Password to confirm this action.";
                        return RedirectToPage();
                    }

                    if (AccountInput.AdminVerifyPassword != adminPassword)
                    {
                        TempData["ErrorMessage"] = "Incorrect Admin Password! Access Denied.";
                        return RedirectToPage();
                    }

                    var resetRequest = new
                    {
                        AdminEmail = adminEmail,
                        AdminPassword = adminPassword,
                        TargetAccountId = AccountInput.AccountId,
                        NewPassword = AccountInput.AccountPassword
                    };

                    var response = await client.PatchAsJsonAsync(
                        "api/account",
                        resetRequest
                    );


                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Password updated successfully!";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Failed to update password.";
                    }
                }
                else
                {
                    TempData["SuccessMessage"] = "Info updated (Simulation only - API endpoint missing for Info Update).";
                }
            }

            return RedirectToPage(); 
        }

        public async Task<IActionResult> OnPostDeleteAccountAsync(short accountId)
        {
            var client = _httpClientFactory.CreateClient("NewsAPI");

            var response = await client.DeleteAsync($"api/account({accountId})");

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Account deleted successfully.";
            }
            else
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] =
                    string.IsNullOrWhiteSpace(errorMsg)
                        ? "Cannot delete account."
                        : errorMsg;
            }

            return RedirectToPage();
        }


        private string BuildKeywordFilter(string keyword)
        {
            keyword = keyword.Trim().ToLower();
            string filter = $"(contains(tolower(AccountName),'{keyword}') or contains(tolower(AccountEmail),'{keyword}')";
            if (keyword == "staff") filter += " or AccountRole eq 1";
            else if (keyword == "lecturer") filter += " or AccountRole eq 2";
            else if (int.TryParse(keyword, out int role)) filter += $" or AccountRole eq {role}";
            filter += ")";
            return filter;
        }

        public class AccountInputModel
        {
            public int AccountId { get; set; }
            public string? AccountName { get; set; }

            public string? AccountEmail { get; set; }

            public string? AccountPassword { get; set; }

            public int? AccountRole { get; set; }

            public string? AdminVerifyPassword { get; set; }
        }
    }
}