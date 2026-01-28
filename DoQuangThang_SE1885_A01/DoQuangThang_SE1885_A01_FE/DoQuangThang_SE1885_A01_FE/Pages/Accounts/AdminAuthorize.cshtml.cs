using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DoQuangThang_SE1885_A01_FE.Pages.Accounts
{
    public abstract class AdminAuthorizeModel : PageModel
    {
        public override void OnPageHandlerExecuting(
       Microsoft.AspNetCore.Mvc.Filters.PageHandlerExecutingContext context)
        {
            var role = context.HttpContext.Session.GetInt32("Role");

            if (role != 0 )
            {
                context.Result = new RedirectToPageResult("/Index");
            }

            base.OnPageHandlerExecuting(context);
        }
    }
}
