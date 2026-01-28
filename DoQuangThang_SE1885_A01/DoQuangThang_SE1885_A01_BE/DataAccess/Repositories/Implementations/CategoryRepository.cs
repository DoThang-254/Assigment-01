using DataAccess.DAO;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories.Implementations
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly FunewsManagementContext _context;
        public CategoryRepository(FunewsManagementContext context)
        {
            _context = context;
        }

        public void CreateCategory(Category category) => CategoryDAO.Instance.Create(_context, category);

        public void DeleteCategory(int? id) => CategoryDAO.Instance.Delete(_context, id);

        public IQueryable<Category> GetCategories() => CategoryDAO.Instance.GetCategories(_context);

        public IQueryable<CategoryArticleCount> GetCategoryArticleCounts() => CategoryDAO.Instance.GetCategoryArticleCounts(_context);

        public Category GetCategoryById(int? id) => CategoryDAO.Instance.GetById(_context, id);

        public void ToggledCategory(int? id) => CategoryDAO.Instance.ToggleActive(_context, id);

        public void UpdateCategory(Category category) => CategoryDAO.Instance.Update(_context, category);

        public short GenerateId() => CategoryDAO.Instance.GenerateNewCategoryId(_context);
    }
}
