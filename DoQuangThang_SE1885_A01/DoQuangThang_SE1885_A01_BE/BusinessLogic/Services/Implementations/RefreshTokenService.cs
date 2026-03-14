using Azure.Core;
using BusinessLogic.Dto;
using BusinessLogic.Services.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Implementations
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ISystemAccountService _accountService;
        public RefreshTokenService(IRefreshTokenRepository refreshTokenRepository, ISystemAccountService accountService)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _accountService = accountService;
        }
        public void AddRefreshTokenAsync(int accountId, string refreshTokenString, string accessTokenId)
        {
            var newRefreshToken = new RefreshToken
            {
                Token = refreshTokenString,
                JwtId = accessTokenId,
                IsUsed = false,
                IsRevoked = false,
                AccountId = (short)accountId,
                AddedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(30)
            };

            _refreshTokenRepository.SaveRefreshToken(newRefreshToken);
        }

        public async Task DeleteTrashTokensAsync(int accountId)
        {
            await _refreshTokenRepository.DeleteTrashTokensAsync(accountId);
        }

        public async Task<ServiceResponse<LoginResponse>> RefreshTokenAsync(string token)
        {
            var storedToken = await _refreshTokenRepository.GetByTokenAsync(token);

            // 2. Các bước kiểm tra bảo mật (Validation Logic chuyển về đây)
            if (storedToken == null)
                return new ServiceResponse<LoginResponse> { Success = false, Message = "Refresh Token is not existed" };

            if (storedToken.IsUsed)
                return new ServiceResponse<LoginResponse> { Success = false, Message = "Refresh Token is used." };

            if (storedToken.IsRevoked)
                return new ServiceResponse<LoginResponse> { Success = false, Message = "Refresh Token is revoked." };

            if (storedToken.ExpiryDate < DateTime.UtcNow)
                return new ServiceResponse<LoginResponse> { Success = false, Message = "Refresh Token is Expired. Please Login Again!" };

            // 3. Đánh dấu token cũ là "Đã dùng"
            storedToken.IsUsed = true;
            storedToken.IsRevoked = true;
            storedToken.RevokedAt = DateTime.UtcNow;
            storedToken.RevokedReason = "Used for Refreshing Token";
            storedToken.ReplacedByToken = token; 

            await _refreshTokenRepository.UpdateAsync(storedToken);

            // 4. Lấy thông tin User (đã Include trong Repo)
            var user = storedToken.SystemAccount;

            // 5. Cấp bộ Token MỚI (Refresh Token Rotation)
            var newAccessToken = _accountService.GenerateJwtToken(user);

            var newRefreshToken = new RefreshToken
            {
                IsUsed = false,
                IsRevoked = false,
                AccountId = user.AccountId,
                AddedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(30),
                Token = Guid.NewGuid().ToString(),
                JwtId = Guid.NewGuid().ToString()
            };

            _refreshTokenRepository.SaveRefreshToken(newRefreshToken);

            return new ServiceResponse<LoginResponse>
            {
                Success = true,
                Data = new LoginResponse
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken.Token,
                    AccountId = user.AccountId,
                    Email = user.AccountEmail,
                    Role = user.AccountRole
                }
            };
        }

        public async Task<bool> RevokeTokenAsync(string token)
        {
            var storedToken = await _refreshTokenRepository.GetByTokenAsync(token);

            if (storedToken == null)
            {
                return false;
            }

            storedToken.IsRevoked = true;
            storedToken.RevokedAt = DateTime.UtcNow;
            storedToken.RevokedReason = "User Logout";
            storedToken.ReplacedByToken = token;
            storedToken.IsUsed = true;


            await _refreshTokenRepository.UpdateAsync(storedToken);

            return true;
        }
    }
}
