using DataAccess.Models;
using DataAccess.Models.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories.Interfaces
{
    public interface IReportRepository
    {
        IQueryable<NewsArticle> GetNewsForReport(
           DateTime? fromDate,
           DateTime? toDate);

        IQueryable<ReportDTO> ReportByCategory(
            DateTime? fromDate,
            DateTime? toDate);

        IQueryable<ReportDTO> ReportByAuthor(
            DateTime? fromDate,
            DateTime? toDate);

        IQueryable<ReportDTO> ReportByStatus(
            DateTime? fromDate,
            DateTime? toDate);

        ReportDTO GetActiveInactiveSummary(
            DateTime? fromDate,
            DateTime? toDate);
    }
}
