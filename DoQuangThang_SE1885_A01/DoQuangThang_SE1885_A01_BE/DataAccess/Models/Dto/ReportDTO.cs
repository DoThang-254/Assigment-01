using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models.Dto
{
    public class ReportDTO
    {
        public int ReportId { get; set; }
        public short? CategoryId { get; set; }
        public short? CreatedById { get; set; }
        public bool? NewsStatus { get; set; }

        public int TotalArticles { get; set; }
        public int ActiveArticles { get; set; }
        public int InactiveArticles { get; set; }
    }
}
