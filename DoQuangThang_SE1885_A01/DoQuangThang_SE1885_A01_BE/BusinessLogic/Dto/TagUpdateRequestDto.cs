using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.Dto
{
    public class TagUpdateRequestDto
    {
        public int TagId { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "TagName cannot exceed 100 characters.")]
        public string TagName { get; set; } = null!;

        [StringLength(500, ErrorMessage = "Note cannot exceed 500 characters.")]
        public string? Note { get; set; }
    }
}
