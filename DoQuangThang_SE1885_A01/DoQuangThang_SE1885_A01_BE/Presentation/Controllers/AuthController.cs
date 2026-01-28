using BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Presentation.ViewModels.Auth;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ISystemAccountService _systemAccountService;
        private readonly INewsArticleService _newsArticleService;

        public AuthController(ISystemAccountService systemAccountService, INewsArticleService newsArticleService)
        {
            _newsArticleService = newsArticleService;
            _systemAccountService = systemAccountService;
        }

        [HttpPost("login")]
        public IActionResult Login(LoginRequest loginRequest)
        {
            var account = _systemAccountService.Login(loginRequest.email, loginRequest.password);
            if (account == null)
            {
                return Unauthorized(new
                {
                    message = "Invalid email or password"
                });
            }

            return Ok(new LoginResponse
            {
                AccountId = account.AccountId,
                Email = account.AccountEmail,
                Role = account.AccountRole
            });
        }

        [HttpPatch("change-password")]
        public IActionResult ChangePassword(int accountId, string oldPassword, string newPassword)
        {
            try
            {
                _systemAccountService.ChangePassword(accountId, oldPassword, newPassword);
                return Ok("Password changed successfully");
            }
            catch
            {
                return BadRequest("Something Went Wrong");
            }
        }

        [HttpGet]
        public IActionResult GetNews(short key)
        {
            try
            {
                var result = _newsArticleService.GetNewsByUserId(key);
                return Ok(result);
            }
            catch
            {
                return BadRequest("Something went wrong");
            }
        }
    }
}
