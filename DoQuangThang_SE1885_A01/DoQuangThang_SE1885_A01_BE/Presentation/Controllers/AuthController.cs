using BusinessLogic.Dto;
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
        private readonly IRefreshTokenService _refreshTokenService; 

        public AuthController(ISystemAccountService systemAccountService, INewsArticleService newsArticleService, IRefreshTokenService refreshTokenService)
        {
            _newsArticleService = newsArticleService;
            _systemAccountService = systemAccountService;
            _refreshTokenService = refreshTokenService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            var account = _systemAccountService.Login(loginRequest.email, loginRequest.password);
            if (account == null)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            await _refreshTokenService.DeleteTrashTokensAsync(account.AccountId);

            var accessTokenString = _systemAccountService.GenerateJwtToken(account);
            var jwtId = Guid.NewGuid().ToString();
            var refreshTokenString = Guid.NewGuid().ToString();

            if (account.AccountId != 0)
            {
                _refreshTokenService.AddRefreshTokenAsync(
                    account.AccountId,
                    refreshTokenString,
                    jwtId
                );
            }

            // 4. Trả về kết quả
            return Ok(new LoginResponse
            {
                AccessToken = accessTokenString,
                // Nếu là Admin (Id=0) thì trả về refreshTokenString nhưng nó sẽ không có tác dụng 
                // khi gọi API refresh-token (vì không có trong DB). 
                // Hoặc bạn có thể để null nếu muốn chặt chẽ.
                RefreshToken = refreshTokenString,

                AccountId = account.AccountId,
                Email = account.AccountEmail,
                Role = account.AccountRole
            });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequest tokenRequest)
        {
            var result = await _refreshTokenService.RefreshTokenAsync(tokenRequest.RefreshToken);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(result.Data);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            var isRevoked = await _refreshTokenService.RevokeTokenAsync(request.RefreshToken);

            return Ok(new { message = "Logout successful, token revoked." });
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
