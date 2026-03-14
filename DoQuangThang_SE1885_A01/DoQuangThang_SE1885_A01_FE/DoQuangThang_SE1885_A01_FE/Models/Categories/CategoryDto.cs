using System.ComponentModel.DataAnnotations;

namespace DoQuangThang_SE1885_A01_FE.Models.Categories
{
    public class CategoryDto
    {
        public short CategoryId { get; set; }

        [Required]
        [StringLength(150)]
        public string CategoryName { get; set; } = null!;
    }
}
