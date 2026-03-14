using BusinessLogic.Services.Interfaces;
using DataAccess.Models.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Implementations
{
    public class ReportService : IReportService
    {
        private readonly DataAccess.Repositories.Interfaces.IReportRepository _reportRepository;
        public ReportService(DataAccess.Repositories.Interfaces.IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }
        public ReportDTO GetActiveInactiveSummary(DateTime? fromDate, DateTime? toDate)
        => _reportRepository.GetActiveInactiveSummary(fromDate, toDate);

        public IQueryable<ReportDTO> ReportByAuthor(DateTime? fromDate, DateTime? toDate)
        => _reportRepository.ReportByAuthor(fromDate, toDate);

        public IQueryable<ReportDTO> ReportByCategory(DateTime? fromDate, DateTime? toDate)
        => _reportRepository.ReportByCategory(fromDate, toDate);

        public IQueryable<ReportDTO> ReportByStatus(DateTime? fromDate, DateTime? toDate)
        => _reportRepository.ReportByStatus(fromDate, toDate);
    }
}
