using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Dto
{
    public class NewsDto
    {
        public string NewsArticleId { get; set; } = null!;
        public string NewsTitle { get; set; }
        public string Headline { get; set; }
        public string NewsContent { get; set; }
        public string NewsSource { get; set; }
        public short CategoryId { get; set; }
        public bool NewsStatus { get; set; } = false;

        public short CreatedById { get; set; }

        public short UpdatedById { get; set; }
        public List<int> TagIds { get; set; } = new();
    }
}
