using BusinessLogic.Dto;
using BusinessLogic.Services.Interfaces;
using DataAccess.DAO;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using System;

namespace BusinessLogic.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public void CreateCategory(CategoryCreateRequestDto category)
        {
            var newCategory = new Category
            {
                CategoryName = category.CategoryName,
                CategoryDescription = category.CategoryDescription,
                ParentCategoryId = category.ParentCategoryId,
                IsActive = true,
            };
            _categoryRepository.CreateCategory(newCategory);
        }

        public void DeleteCategory(int? id)
        {
            _categoryRepository.DeleteCategory(id);
        }

        public IQueryable<Category> GetCategories()
        {
            return _categoryRepository.GetCategories();
        }

        public IQueryable<CategoryArticleCount> GetCategoryArticleCounts()
        {
           return _categoryRepository.GetCategoryArticleCounts();
        }

        public Category GetCategoryById(int? id)
        {
            return _categoryRepository.GetCategoryById(id);
        }

        public void ToggleCategory(int? id)
        {
            if(!id.HasValue) 
                throw new ArgumentNullException("Category ID cannot be null.");
            _categoryRepository.ToggledCategory(id);
        }

        public void UpdateCategory(CategoryUpdateRequestDto category)
        {
            // fetch existing to compare ParentCategoryId
            var existing = _categoryRepository.GetCategoryById(category.CategoryId);
            if (existing == null)
                throw new ArgumentException("Category not found.", nameof(category.CategoryId));

            // service-level validation to prevent throwing from DAO for expected rules
            bool isUsed = _categoryRepository.IsCategoryUsed(category.CategoryId);
            if (isUsed && existing.ParentCategoryId != category.ParentCategoryId)
            {
                // throw an appropriate service-level exception
                throw new InvalidOperationException("Cannot change ParentCategoryID because category is already used by articles.");
            }

            var updatedCategory = new Category
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                CategoryDescription = category.CategoryDesciption,
                ParentCategoryId = category.ParentCategoryId,
                IsActive = category.IsActive,
            };
            _categoryRepository.UpdateCategory(updatedCategory);
        }

       
    }
}
