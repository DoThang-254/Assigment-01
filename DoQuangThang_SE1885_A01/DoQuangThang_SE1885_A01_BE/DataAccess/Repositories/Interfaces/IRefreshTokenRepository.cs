using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories.Interfaces
{
    public interface IRefreshTokenRepository
    {
        void SaveRefreshToken(RefreshToken refreshToken);
        Task<RefreshToken> GetByTokenAsync(string token);
        Task UpdateAsync(RefreshToken refreshToken);
        Task DeleteTrashTokensAsync(int accountId);
    }
}
