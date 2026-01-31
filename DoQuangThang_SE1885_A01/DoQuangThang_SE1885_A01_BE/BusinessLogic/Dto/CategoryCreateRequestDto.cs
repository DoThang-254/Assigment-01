using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.Dto
{
    public class CategoryCreateRequestDto
    {
        [Required]
        [StringLength(200, ErrorMessage = "CategoryName cannot exceed 200 characters.")]
        public string CategoryName { get; set; } = null!;

        [Required]
        [StringLength(1000, ErrorMessage = "CategoryDescription cannot exceed 1000 characters.")]
        public string CategoryDescription { get; set; } = null!;

        [Range(1, short.MaxValue, ErrorMessage = "ParentCategoryId must be a positive value.")]
        public short? ParentCategoryId { get; set; }
    }
}
