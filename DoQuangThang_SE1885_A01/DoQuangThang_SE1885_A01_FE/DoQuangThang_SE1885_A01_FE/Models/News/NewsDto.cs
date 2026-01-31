using DoQuangThang_SE1885_A01_FE.Models.Accounts;
using DoQuangThang_SE1885_A01_FE.Models.Categories;
using DoQuangThang_SE1885_A01_FE.Models.Tags;
using System.ComponentModel.DataAnnotations;

namespace DoQuangThang_SE1885_A01_FE.Models.News
{
    public class NewsDto
    {
        [Required]
        public string NewsArticleId { get; set; } = null!;

        [Required]
        [StringLength(200)]
        public string NewsTitle { get; set; } = null!;

        [StringLength(500)]
        public string Headline { get; set; } = null!;

        [Required]
        [MinLength(10)]
        [StringLength(10000)]
        public string NewsContent { get; set; } = null!;

        [StringLength(500)]
        [Url]
        public string NewsSource { get; set; } = null!;

        [Range(1, short.MaxValue)]
        public short CategoryId { get; set; }

        public bool NewsStatus { get; set; } = false;

        [DataType(DataType.DateTime)]
        public DateTime? CreatedDate { get; set; }

        public short? CreatedById { get; set; }

        public short? UpdatedById { get; set; }

        public AccountsDto? CreatedBy { get; set; }

        public CategoryDto Category { get; set; } = null!;

        public List<TagDto> Tags { get; set; } = new();

        public List<int> TagIds { get; set; } = new();
    }
}
