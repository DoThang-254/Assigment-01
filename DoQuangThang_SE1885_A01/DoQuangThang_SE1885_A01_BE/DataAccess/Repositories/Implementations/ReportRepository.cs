using DataAccess.DAO;
using DataAccess.Models;
using DataAccess.Models.Dto;
using DataAccess.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories.Implementations
{
    public class ReportRepository : IReportRepository
    {
        private readonly FunewsManagementContext _context;

        public ReportRepository(FunewsManagementContext context)
        {
            _context = context;
        }

        public ReportDTO GetActiveInactiveSummary(DateTime? fromDate, DateTime? toDate)
        => ReportDAO.Instance.GetActiveInactiveSummary(_context, fromDate, toDate);

        public IQueryable<NewsArticle> GetNewsForReport(DateTime? fromDate, DateTime? toDate)
        => ReportDAO.Instance.GetNewsForReport(_context, fromDate, toDate);

        public IQueryable<ReportDTO> ReportByAuthor(DateTime? fromDate, DateTime? toDate)
        => ReportDAO.Instance.ReportByAuthor(_context, fromDate, toDate);

        public IQueryable<ReportDTO> ReportByCategory(DateTime? fromDate, DateTime? toDate)
        => ReportDAO.Instance.ReportByCategory(_context, fromDate, toDate);

        public IQueryable<ReportDTO> ReportByStatus(DateTime? fromDate, DateTime? toDate)
        => ReportDAO.Instance.ReportByStatus(_context, fromDate, toDate);
    }
}
