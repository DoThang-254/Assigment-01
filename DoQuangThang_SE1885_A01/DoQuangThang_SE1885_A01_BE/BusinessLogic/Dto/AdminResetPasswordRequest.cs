using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.Dto
{
    public class AdminResetPasswordRequest
    {
        [Required(ErrorMessage = "AdminEmail is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        [StringLength(256, ErrorMessage = "AdminEmail cannot exceed 256 characters.")]
        public string AdminEmail { get; set; } = null!;

        [Required(ErrorMessage = "AdminPassword is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "AdminPassword must be between 6 and 100 characters.")]
        public string AdminPassword { get; set; } = null!;

        [Required(ErrorMessage = "TargetAccountId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "TargetAccountId must be a positive integer.")]
        public int TargetAccountId { get; set; }

        [Required(ErrorMessage = "NewPassword is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "NewPassword must be between 6 and 100 characters.")]
        public string NewPassword { get; set; } = null!;
    }
}
