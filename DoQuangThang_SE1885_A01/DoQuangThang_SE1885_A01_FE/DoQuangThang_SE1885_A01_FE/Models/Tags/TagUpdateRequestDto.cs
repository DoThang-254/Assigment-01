using System.ComponentModel.DataAnnotations;

namespace DoQuangThang_SE1885_A01_FE.Models.Tags
{
    public class TagUpdateRequestDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "TagId must be a positive integer.")]
        public int TagId { get; set; }

        [Required(ErrorMessage = "Tag name is required.")]
        [StringLength(100, ErrorMessage = "Tag name cannot exceed {1} characters.")]
        public string TagName { get; set; } = null!;

        [StringLength(1000, ErrorMessage = "Note cannot exceed {1} characters.")]
        public string? Note { get; set; }
    }
}