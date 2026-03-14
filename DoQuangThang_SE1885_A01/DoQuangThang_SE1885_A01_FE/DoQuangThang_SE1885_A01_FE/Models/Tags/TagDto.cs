using System.ComponentModel.DataAnnotations;

namespace DoQuangThang_SE1885_A01_FE.Models.Tags
{
    public class TagDto
    {
        public int TagId { get; set; }

        [Required]
        [StringLength(100)]
        public string TagName { get; set; } = null!;

        [StringLength(1000)]
        public string? Note { get; set; }
    }
}
