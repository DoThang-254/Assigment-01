using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Dto
{
    public class CategoryCreateRequestDto
    {
        public string CategoryName { get; set; } = null!;

        public string CategoryDescription { get; set; } = null!;

        public short? ParentCategoryId { get; set; }
    }
}
