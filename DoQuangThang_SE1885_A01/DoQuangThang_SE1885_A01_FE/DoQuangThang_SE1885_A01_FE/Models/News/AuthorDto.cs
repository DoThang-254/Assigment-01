using System.ComponentModel.DataAnnotations;

namespace DoQuangThang_SE1885_A01_FE.Models.News
{
    public class AuthorDto
    {
        [Range(1, short.MaxValue, ErrorMessage = "AuthorId must be a positive value.")]
        public short? AuthorId { get; set; }

        [Required(ErrorMessage = "Author name is required.")]
        [StringLength(150, ErrorMessage = "Author name cannot exceed {1} characters.")]
        public string AuthorName { get; set; } = null!;
    }
}
