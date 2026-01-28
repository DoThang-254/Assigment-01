using BusinessLogic.Services.Implementations;
using BusinessLogic.Services.Interfaces;
using DataAccess.DAO;
using DataAccess.Models;
using DataAccess.Models.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Presentation.Controllers
{
    [Route("api/report")]
    public class reportController : ODataController
    {
        private readonly IReportService _reportService;

        public reportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [EnableQuery]
        [HttpGet("ByCategory")]
        public IQueryable<ReportDTO> ByCategory(DateTime? fromDate, DateTime? toDate)
        {
            return _reportService.ReportByCategory(fromDate, toDate);
        }

        [EnableQuery]
        [HttpGet("ByAuthor")]
        public IQueryable<ReportDTO> ByAuthor(DateTime? fromDate, DateTime? toDate)
        {
            return _reportService.ReportByAuthor(fromDate, toDate);
        }

        [EnableQuery]
        [HttpGet("ByStatus")]
        public IQueryable<ReportDTO> ByStatus(DateTime? fromDate, DateTime? toDate)
        {
            return _reportService.ReportByStatus(fromDate, toDate);
        }

        [HttpGet("Summary")]
        public ReportDTO Summary(DateTime? fromDate, DateTime? toDate)
        {
            return _reportService.GetActiveInactiveSummary(fromDate, toDate);
        }
    }

}
