using BusinessLogic.Dto.Analytics;
using BusinessLogic.Services.Interfaces;
using ClosedXML.Excel;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Implementations
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IAnalyticsRepository _analyticsRepository;

        public AnalyticsService(IAnalyticsRepository analyticsRepository)
        {
            _analyticsRepository = analyticsRepository;
        }
        private IQueryable<NewsArticle> ApplyFilter(IQueryable<NewsArticle> query, DashboardFilterDto filter)
        {
            if (filter.StartDate.HasValue)
                query = query.Where(x => x.CreatedDate >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(x => x.CreatedDate <= filter.EndDate.Value);

            if (filter.CategoryId.HasValue)
                query = query.Where(x => x.CategoryId == filter.CategoryId.Value);

            if (filter.AuthorId.HasValue)
                query = query.Where(x => x.CreatedById == filter.AuthorId.Value);

            return query;
        }

        public async Task<DashboardDto> GetDashboardStatsAsync(DashboardFilterDto filter)
        {
            var query = _analyticsRepository.GetNewsQuery();

            // 1. Áp dụng filter ngày tháng (nếu có)
            query = ApplyFilter(query, filter);

            var dashboardData = new DashboardDto();

            // 2. Tính toán các con số tổng quan
            dashboardData.TotalArticles = await query.CountAsync();
            dashboardData.TotalViews = await query.SumAsync(x => x.Views ?? 0);
            dashboardData.TotalActiveAccounts = await _analyticsRepository.CountActiveAccountsAsync();

            // 3. Data cho Pie Chart (Group by Category)
            dashboardData.ArticlesByCategory = await query
                .GroupBy(x => x.Category.CategoryName)
                .Select(g => new ChartDataDto
                {
                    Label = g.Key ?? "Unknown",
                    Value = g.Count()
                })
                .ToListAsync();

            // 4. Data cho Bar Chart (Group by Date)
            dashboardData.ArticlesByDate = await query
                .Where(x => x.CreatedDate != null)
                .GroupBy(x => x.CreatedDate.Value.Date) // Gom nhóm theo ngày
                .OrderBy(g => g.Key)
                .Select(g => new ChartDataDto
                {
                    Label = g.Key.ToString("dd/MM/yyyy"),
                    Value = g.Count()
                })
                .ToListAsync();

            // 5. Data cho Pie Chart (Group by Status)
            dashboardData.ArticlesByStatus = await query
                .Where(x => x.NewsStatus != null) // Lọc bỏ null trước
                .GroupBy(x => x.NewsStatus)
                .Select(g => new ChartDataDto
                {
                    // SỬA Ở ĐÂY: Kiểm tra bool để gán chuỗi tương ứng
                    Label = g.Key == true ? "Active" : "Inactive",
                    Value = g.Count()
                })
                .ToListAsync();

            return dashboardData;
        }

        public async Task<List<TrendingArticleDto>> GetTrendingArticlesAsync(int top = 5)
        {
            // Logic Trending: Lấy bài Active -> Sắp xếp View giảm dần -> Lấy Top N
            return await _analyticsRepository.GetNewsQuery()
                .Where(x => x.NewsStatus == true)
                .OrderByDescending(x => x.Views) // Sắp xếp theo Views
                .Take(top)
                .Select(x => new TrendingArticleDto
                {
                    NewsArticleId = x.NewsArticleId,
                    NewsTitle = x.NewsTitle,
                    CategoryName = x.Category.CategoryName,
                    AuthorName = x.CreatedBy.AccountName,
                    CreatedDate = x.CreatedDate ?? DateTime.Now,
                    Views = x.Views ?? 0,
                    NewsImage = x.NewsImage
                })
                .ToListAsync();
        }

        public async Task<byte[]> ExportNewsReportAsync(DashboardFilterDto filter)
        {
            // Lấy dữ liệu raw (đã filter)
            var query = _analyticsRepository.GetNewsQuery();
            query = ApplyFilter(query, filter);

            var data = await query.Select(x => new {
                x.NewsArticleId,
                x.NewsTitle,
                Category = x.Category.CategoryName,
                Author = x.CreatedBy.AccountName,
                Date = x.CreatedDate,
                Views = x.Views
            }).ToListAsync();

            // Tạo file Excel bằng ClosedXML
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("News Statistics");

                // Header
                worksheet.Cell(1, 1).Value = "ID";
                worksheet.Cell(1, 2).Value = "Title";
                worksheet.Cell(1, 3).Value = "Category";
                worksheet.Cell(1, 4).Value = "Author";
                worksheet.Cell(1, 5).Value = "Created Date";
                worksheet.Cell(1, 6).Value = "Views";
                var header = worksheet.Range("A1:F1");
                header.Style.Font.Bold = true;
                header.Style.Fill.BackgroundColor = XLColor.LightGray;

                // Data
                int row = 2;
                foreach (var item in data)
                {
                    worksheet.Cell(row, 1).Value = item.NewsArticleId;
                    worksheet.Cell(row, 2).Value = item.NewsTitle;
                    worksheet.Cell(row, 3).Value = item.Category;
                    worksheet.Cell(row, 4).Value = item.Author;
                    worksheet.Cell(row, 5).Value = item.Date;
                    worksheet.Cell(row, 6).Value = item.Views;
                    row++;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

        public async Task<List<TrendingArticleDto>> GetRelatedArticlesAsync(string newsId, int top = 5)
        {
            var sourceArticle = await _analyticsRepository.GetNewsQuery()
                        .FirstOrDefaultAsync(x => x.NewsArticleId == newsId);

            if (sourceArticle == null) return new List<TrendingArticleDto>();

            var sourceTagIds = sourceArticle.Tags.Select(t => t.TagId).ToList();

            // B2: Query bài liên quan
            var query = _analyticsRepository.GetNewsQuery()
                .Where(x => x.NewsStatus == true)          // Chỉ lấy bài Active
                .Where(x => x.NewsArticleId != newsId)     // Trừ bài hiện tại ra
                .Where(x =>
                    x.CategoryId == sourceArticle.CategoryId // Cùng danh mục
                    ||
                    x.Tags.Any(t => sourceTagIds.Contains(t.TagId)) // HOẶC Có chứa ít nhất 1 Tag trùng
                );

            // B3: Chọn top N bài mới nhất và Map sang DTO
            return await query
                .OrderByDescending(x => x.CreatedDate)
                .Take(top)
                .Select(x => new TrendingArticleDto
                {
                    NewsArticleId = x.NewsArticleId,
                    NewsTitle = x.NewsTitle,
                    NewsImage = x.NewsImage,
                    CategoryName = x.Category.CategoryName,
                    AuthorName = x.CreatedBy.AccountName,
                    CreatedDate = x.CreatedDate ?? DateTime.Now,
                    Views = x.Views ?? 0,
                    Tags = x.Tags.Select(t => t.TagName).ToList()
                })
                .ToListAsync();
        }
    }
}

