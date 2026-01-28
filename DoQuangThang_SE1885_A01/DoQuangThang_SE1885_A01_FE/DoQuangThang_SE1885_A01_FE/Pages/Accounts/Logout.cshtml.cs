using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DoQuangThang_SE1885_A01_FE.Pages.Accounts
{
    public class LogoutModel : PageModel
    {
        public IActionResult OnGet()
        {
            HttpContext.Session.Clear();    
            return RedirectToPage("/Index"); 
        }
    }
}
