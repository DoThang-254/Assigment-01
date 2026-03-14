using BusinessLogic.Dto.Analytics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Interfaces
{
    public interface IAnalyticsService
    {
        Task<DashboardDto> GetDashboardStatsAsync(DashboardFilterDto filter);
        Task<List<TrendingArticleDto>> GetTrendingArticlesAsync(int top = 5);
        Task<byte[]> ExportNewsReportAsync(DashboardFilterDto filter);

        Task<List<TrendingArticleDto>> GetRelatedArticlesAsync(string newsId, int top = 5);
    }
}
