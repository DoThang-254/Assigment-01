using DataAccess.Models;
using DataAccess.Models.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DAO
{
    public class ReportDAO
    {
        private static ReportDAO? instance = null;
        private static readonly object instanceLock = new object();

        private ReportDAO() { }

        public static ReportDAO Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new ReportDAO();
                    }
                    return instance;
                }
            }
        }

        // Base query cho Report (dùng cho OData)
        public IQueryable<NewsArticle> GetNewsForReport(
            FunewsManagementContext context,
            DateTime? fromDate,
            DateTime? toDate)
        {
            IQueryable<NewsArticle> query = context.NewsArticles;

            if (fromDate.HasValue)
            {
                query = query.Where(n => n.CreatedDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(n => n.CreatedDate <= toDate.Value);
            }

            return query.OrderByDescending(n => n.CreatedDate);
        }

        // Report theo Category
        public IQueryable<ReportDTO> ReportByCategory(
            FunewsManagementContext context,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var query = GetNewsForReport(context, fromDate, toDate);

            return query
                .GroupBy(n => n.CategoryId)
                .OrderByDescending(g => g.Max(n => n.CreatedDate))
                .Select(g => new ReportDTO
                {
                    CategoryId = g.Key,
                    TotalArticles = g.Count(),
                    ActiveArticles = g.Count(n => n.NewsStatus == true),
                    InactiveArticles = g.Count(n => n.NewsStatus == false)
                });
        }

        // Report theo Author (CreatedByID)
        public IQueryable<ReportDTO> ReportByAuthor(
            FunewsManagementContext context,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var query = GetNewsForReport(context, fromDate, toDate);

            return query
                .GroupBy(n => n.CreatedById)
                .OrderByDescending(g => g.Max(n => n.CreatedDate))
                .Select(g => new ReportDTO
                {
                    CreatedById = g.Key,
                    TotalArticles = g.Count(),
                    ActiveArticles = g.Count(n => n.NewsStatus == true),
                    InactiveArticles = g.Count(n => n.NewsStatus == false)
                });
        }

        // Report theo Status
        public IQueryable<ReportDTO> ReportByStatus(
            FunewsManagementContext context,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var query = GetNewsForReport(context, fromDate, toDate);

            return query
                .GroupBy(n => n.NewsStatus)
                .OrderByDescending(g => g.Max(n => n.CreatedDate))
                .Select(g => new ReportDTO
                {
                    NewsStatus = g.Key,
                    TotalArticles = g.Count(),
                    ActiveArticles = g.Count(n => n.NewsStatus == true),
                    InactiveArticles = g.Count(n => n.NewsStatus == false)
                });
        }

        // Tổng Active vs Inactive (không group)
        public ReportDTO GetActiveInactiveSummary(
            FunewsManagementContext context,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var query = GetNewsForReport(context, fromDate, toDate);

            return new ReportDTO
            {
                TotalArticles = query.Count(),
                ActiveArticles = query.Count(n => n.NewsStatus == true),
                InactiveArticles = query.Count(n => n.NewsStatus == false)
            };
        }
    }
}
