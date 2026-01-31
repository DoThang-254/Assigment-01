using BusinessLogic.Services.Implementations;
using BusinessLogic.Services.Interfaces;
using DataAccess.DAO;
using DataAccess.Models;
using DataAccess.Models.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using System.Linq;
using System.IO;
using System.Text;
using System.Globalization;
using CsvHelper;

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

        private bool WantsCsv()
        {
            var accept = Request.Headers["Accept"].ToString();
            return !string.IsNullOrEmpty(accept) && accept.Contains("text/csv", System.StringComparison.OrdinalIgnoreCase);
        }

        private FileContentResult CsvFile<T>(IEnumerable<T> records, string fileName)
        {
            using var ms = new MemoryStream();
            using (var writer = new StreamWriter(ms, Encoding.UTF8, leaveOpen: true))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(records);
                writer.Flush();
            }

            var bytes = ms.ToArray();
            return File(bytes, "text/csv; charset=utf-8", fileName);
        }

        [EnableQuery]
        [HttpGet("ByCategory")]
        [Produces("application/json", "application/xml", "text/csv")]
        public ActionResult ByCategory(DateTime? fromDate, DateTime? toDate)
        {
            var query = _reportService.ReportByCategory(fromDate, toDate);

            if (WantsCsv())
            {
                var list = query.ToList(); // materialize for CSV serialization
                return CsvFile(list, "report_by_category.csv");
            }

            return Ok(query);
        }

        [EnableQuery]
        [HttpGet("ByAuthor")]
        [Produces("application/json", "application/xml", "text/csv")]
        public ActionResult ByAuthor(DateTime? fromDate, DateTime? toDate)
        {
            var query = _reportService.ReportByAuthor(fromDate, toDate);

            if (WantsCsv())
            {
                var list = query.ToList();
                return CsvFile(list, "report_by_author.csv");
            }

            return Ok(query);
        }

        [EnableQuery]
        [HttpGet("ByStatus")]
        [Produces("application/json", "application/xml", "text/csv")]
        public ActionResult ByStatus(DateTime? fromDate, DateTime? toDate)
        {
            var query = _reportService.ReportByStatus(fromDate, toDate);

            if (WantsCsv())
            {
                var list = query.ToList();
                return CsvFile(list, "report_by_status.csv");
            }

            return Ok(query);
        }

        [HttpGet("Summary")]
        [Produces("application/json", "application/xml", "text/csv")]
        public ActionResult Summary(DateTime? fromDate, DateTime? toDate)
        {
            var summary = _reportService.GetActiveInactiveSummary(fromDate, toDate);

            if (WantsCsv())
            {
                // single record → return as one-row CSV
                return CsvFile(new[] { summary }, "report_summary.csv");
            }
                
            return Ok(summary);
        }
    }
}
