using System.ComponentModel.DataAnnotations;

namespace Presentation.ViewModels.Auth
{
    public class CreateAccountRequest
    {
        [Required(ErrorMessage = "AccountName is required.")]
        [StringLength(100, ErrorMessage = "AccountName cannot exceed 100 characters.")]
        public string? AccountName { get; set; }

        [Required(ErrorMessage = "AccountEmail is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        [StringLength(256, ErrorMessage = "AccountEmail cannot exceed 256 characters.")]
        public string? AccountEmail { get; set; }

        [Required(ErrorMessage = "AccountPassword is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "AccountPassword must be between 6 and 100 characters.")]
        public string? AccountPassword { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "AccountRole must be a non-negative integer.")]
        public int? AccountRole { get; set; }
    }
}
