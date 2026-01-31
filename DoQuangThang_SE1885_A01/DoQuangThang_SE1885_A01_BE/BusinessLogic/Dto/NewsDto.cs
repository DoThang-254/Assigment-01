using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.Dto
{
    public class NewsDto
    {
        [Required(ErrorMessage = "NewsArticleId is required.")]
        [StringLength(50, ErrorMessage = "NewsArticleId cannot exceed 50 characters.")]
        public string NewsArticleId { get; set; } = null!;

        [Required(ErrorMessage = "NewsTitle is required.")]
        [StringLength(200, ErrorMessage = "NewsTitle cannot exceed 200 characters.")]
        public string NewsTitle { get; set; } = null!;

        [StringLength(500, ErrorMessage = "Headline cannot exceed 500 characters.")]
        public string Headline { get; set; } = null!;

        [Required(ErrorMessage = "NewsContent is required.")]
        [MinLength(10, ErrorMessage = "NewsContent must be at least 10 characters.")]
        public string NewsContent { get; set; } = null!;

        [StringLength(200, ErrorMessage = "NewsSource cannot exceed 200 characters.")]
        public string NewsSource { get; set; } = null!;

        [Required(ErrorMessage = "CategoryId is required.")]
        [Range(1, short.MaxValue, ErrorMessage = "CategoryId must be a positive value.")]
        public short CategoryId { get; set; }

        public bool NewsStatus { get; set; } = false;

        [Required(ErrorMessage = "CreatedById is required.")]
        [Range(1, short.MaxValue, ErrorMessage = "CreatedById must be a positive value.")]
        public short CreatedById { get; set; }

        [Range(1, short.MaxValue, ErrorMessage = "UpdatedById must be a positive value.")]
        public short UpdatedById { get; set; }

        [Required]
        public List<int> TagIds { get; set; } = new();
    }
}
