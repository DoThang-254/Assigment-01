using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Interfaces
{
    public interface IReportService
    {
        IQueryable<DataAccess.Models.Dto.ReportDTO> ReportByCategory(DateTime? fromDate, DateTime? toDate);
        IQueryable<DataAccess.Models.Dto.ReportDTO> ReportByAuthor(DateTime? fromDate, DateTime? toDate);
        IQueryable<DataAccess.Models.Dto.ReportDTO> ReportByStatus(DateTime? fromDate, DateTime? toDate);
        DataAccess.Models.Dto.ReportDTO GetActiveInactiveSummary(DateTime? fromDate, DateTime? toDate);
    }
}
