using BusinessLogic.Dto;
using DataAccess.DAO;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Interfaces
{
    public interface ICategoryService
    {
        IQueryable<Category> GetCategories();

        Category GetCategoryById(int? id);

        void CreateCategory(CategoryCreateRequestDto category);

        void UpdateCategory(CategoryUpdateRequestDto category);

        void DeleteCategory(int? id);

        void ToggleCategory(int? id);

        IQueryable<CategoryArticleCount> GetCategoryArticleCounts();
    }
}
