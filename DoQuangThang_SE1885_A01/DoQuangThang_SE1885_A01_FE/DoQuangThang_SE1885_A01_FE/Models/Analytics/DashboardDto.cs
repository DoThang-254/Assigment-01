namespace DoQuangThang_SE1885_A01_FE.Models.Analytics
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

    // Class hỗ trợ vẽ biểu đồ
    public class ChartDataDto
    {
        public string Label { get; set; }
        public int Value { get; set; }
    }

    // Dữ liệu bài viết Trending
    public class TrendingArticleDto
    {
        public string NewsArticleId { get; set; }
        public string NewsTitle { get; set; }
        public string CategoryName { get; set; }
        public string AuthorName { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Views { get; set; }
    }
}
