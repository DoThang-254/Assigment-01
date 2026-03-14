using BusinessLogic.Dto;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Interfaces
{
    public interface IRefreshTokenService
    {
        void AddRefreshTokenAsync(int accountId, string refreshTokenString, string accessTokenId);
        Task<ServiceResponse<LoginResponse>> RefreshTokenAsync(string token);
        Task<bool> RevokeTokenAsync(string token);

        Task DeleteTrashTokensAsync(int accountId);
    }
}
