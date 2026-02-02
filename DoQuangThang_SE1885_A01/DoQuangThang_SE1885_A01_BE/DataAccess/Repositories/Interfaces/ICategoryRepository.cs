using DataAccess.DAO;
using DataAccess.Models;
using System.Linq;

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

        // new method to allow service-level validation
        bool IsCategoryUsed(int? id);
    }
}
