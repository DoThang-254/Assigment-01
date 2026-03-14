using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.Dto
{
    public class TagDto
    {
        [Required]
        [StringLength(100, ErrorMessage = "TagName cannot exceed 100 characters.")]
        public string? TagName { get; set; }

        [StringLength(500, ErrorMessage = "Note cannot exceed 500 characters.")]
        public string? Note { get; set; }
    }
}
