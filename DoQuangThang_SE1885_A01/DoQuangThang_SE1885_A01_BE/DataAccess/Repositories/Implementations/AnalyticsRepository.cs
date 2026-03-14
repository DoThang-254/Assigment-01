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
    public class AnalyticsRepository : IAnalyticsRepository
    {
        private readonly FunewsManagementContext _context;

        public AnalyticsRepository(FunewsManagementContext context)
        {
            _context = context;
        }

        public IQueryable<NewsArticle> GetNewsQuery()
        {
            return _context.NewsArticles
                .Include(n => n.Category)
                .Include(n => n.CreatedBy)
                .Include(n => n.Tags)
                .AsQueryable();
        }

        public async Task<int> CountActiveAccountsAsync()
        {
            return await _context.SystemAccounts.CountAsync();
        }
    }
}
