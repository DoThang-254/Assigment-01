using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Dto.Analytics
{
    public class DashboardDto
    {
        public int TotalArticles { get; set; }
        public int TotalViews { get; set; }
        public int TotalActiveAccounts { get; set; }

        public List<ChartDataDto> ArticlesByCategory { get; set; } = new();

        public List<ChartDataDto> ArticlesByDate { get; set; } = new();

        public List<ChartDataDto> ArticlesByStatus { get; set; } = new();

    }

    public class ChartDataDto
    {
        public string Label { get; set; } 
        public int Value { get; set; }    
    }

    public class TrendingArticleDto
    {
        public string NewsArticleId { get; set; } 
        public string NewsTitle { get; set; }
        public string CategoryName { get; set; }
        public string AuthorName { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Views { get; set; }       
        public string? NewsImage { get; set; }

        public List<string> Tags { get; set; } = new(); 
    }

    public class DashboardFilterDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public short? CategoryId { get; set; }
        public short? AuthorId { get; set; }   
    }
}
