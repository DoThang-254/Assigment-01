using DoQuangThang_SE1885_A01_FE.Models.Accounts;
using DoQuangThang_SE1885_A01_FE.Models.Categories;
using DoQuangThang_SE1885_A01_FE.Models.Tags;

namespace DoQuangThang_SE1885_A01_FE.Models.News
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
        public DateTime? CreatedDate { get; set; }

        public short? CreatedById { get; set; }

        public short? UpdatedById { get; set; } 
        public AccountsDto CreatedBy { get; set; }

        public CategoryDto Category { get; set; } = null!;

        public List<TagDto> Tags { get; set; } = new();

        public List<int> TagIds { get; set; } = new();
    }
}
