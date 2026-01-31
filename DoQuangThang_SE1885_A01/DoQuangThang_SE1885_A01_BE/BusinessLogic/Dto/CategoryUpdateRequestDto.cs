using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.Dto
{
    public class CategoryUpdateRequestDto
    {
        [Required]
        [Range(1, short.MaxValue, ErrorMessage = "CategoryId must be a positive value.")]
        public short CategoryId { get; set; }

        [Required]
        [StringLength(200, ErrorMessage = "CategoryName cannot exceed 200 characters.")]
        public string CategoryName { get; set; } = null!;

        [Required]
        [StringLength(1000, ErrorMessage = "CategoryDesciption cannot exceed 1000 characters.")]
        public string CategoryDesciption { get; set; } = null!;

        [Range(1, short.MaxValue, ErrorMessage = "ParentCategoryId must be a positive value.")]
        public short? ParentCategoryId { get; set; }

        public bool? IsActive { get; set; }
    }
}
