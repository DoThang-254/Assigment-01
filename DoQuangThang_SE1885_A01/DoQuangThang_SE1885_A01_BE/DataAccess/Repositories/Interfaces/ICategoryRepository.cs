using DataAccess.DAO;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories.Interfaces
{
    public interface ICategoryRepository
    {
        IQueryable<Category> GetCategories();

        Category GetCategoryById(int? id);

        void CreateCategory(Category category);
        void UpdateCategory(Category category);
        void DeleteCategory(int? id);
        void ToggledCategory(int? id);

        IQueryable<CategoryArticleCount> GetCategoryArticleCounts();

        short GenerateId();
    }
}
