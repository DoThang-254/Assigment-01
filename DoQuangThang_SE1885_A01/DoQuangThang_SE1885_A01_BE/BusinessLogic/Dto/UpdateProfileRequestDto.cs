using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.Dto
{
    public class UpdateProfileRequestDto
    {
        [StringLength(100, ErrorMessage = "AccountName cannot exceed 100 characters.")]
        public string? AccountName { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        [StringLength(256, ErrorMessage = "AccountEmail cannot exceed 256 characters.")]
        public string? AccountEmail { get; set; }
    }
}
