using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories.Implementations
{
    public class RefreshRepository : IRefreshTokenRepository
    {
        private readonly FunewsManagementContext _context;

        public RefreshRepository(FunewsManagementContext context)
        {
            _context = context;
        }

        public async Task DeleteTrashTokensAsync(int accountId)
        {
            var trashTokens = _context.RefreshTokens
                        .Where(x => x.AccountId == accountId &&
                                   (x.IsRevoked || x.ExpiryDate <= DateTime.UtcNow));

            
            _context.RefreshTokens.RemoveRange(trashTokens);

            await _context.SaveChangesAsync();
        }

        public async Task<RefreshToken> GetByTokenAsync(string token)
        {
            return await _context.RefreshTokens
                        .Include(t => t.SystemAccount) 
                        .FirstOrDefaultAsync(x => x.Token == token);
        }

        public void SaveRefreshToken(RefreshToken refreshToken)
        {
            _context.RefreshTokens.Add(refreshToken);
            _context.SaveChanges();
        }

        public async Task UpdateAsync(RefreshToken refreshToken)
        {
            _context.RefreshTokens.Update(refreshToken);
            await _context.SaveChangesAsync();
        }
    }
}
