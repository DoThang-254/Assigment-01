using DataAccess.Models;
using System;
using System.Linq;

namespace DataAccess.DAO
{
    public class CategoryDAO
    {
        private static CategoryDAO? instance = null;
        private static readonly object instanceLock = new object();
        private CategoryDAO()
        {
        }
        public static CategoryDAO Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new CategoryDAO();
                    }
                    return instance;
                }
            }
        }

        public IQueryable<Category> GetCategories(FunewsManagementContext context)
        {
            return context.Categories;
        }


        public Category? GetById(FunewsManagementContext context, int? id)
        {
            return context.Categories
                .FirstOrDefault(c => c.CategoryId == id);
        }

        public void Create(FunewsManagementContext context, Category category)
        {
            context.Categories.Add(category);
            context.SaveChanges();
        }

        // =====================================================
        // 4. UPDATE
        // - Không được đổi ParentCategoryID nếu đã có bài viết
        // =====================================================
        public void Update(FunewsManagementContext context, Category updated)
        {
            var category = context.Categories
                .FirstOrDefault(c => c.CategoryId == updated.CategoryId);

            if (category == null)
                throw new Exception("Category not found.");

            bool isUsed = context.NewsArticles
                .Any(n => n.CategoryId == category.CategoryId);

            if (isUsed && category.ParentCategoryId != updated.ParentCategoryId)
            {
                // throw a specific exception type from DataAccess (defense-in-depth)
                throw new Exception(
                    "Cannot change ParentCategoryID because category is already used by articles."
                );
            }

            category.CategoryName = updated.CategoryName;
            category.CategoryDescription = updated.CategoryDescription;
            category.ParentCategoryId = updated.ParentCategoryId;
            category.IsActive = updated.IsActive ?? false;

            context.SaveChanges();
        }

        // New helper: check usage (used by service layer)
        public bool IsCategoryUsed(FunewsManagementContext context, int? categoryId)
        {
            return context.NewsArticles.Any(n => n.CategoryId == categoryId);
        }

        // =====================================================
        // 5. DELETE
        // - Chỉ xoá nếu KHÔNG có bài viết
        // =====================================================
        public void Delete(FunewsManagementContext context, int? categoryId)
        {
            bool isUsed = context.NewsArticles
                .Any(n => n.CategoryId == categoryId);

            if (isUsed)
            {
                throw new Exception(
                    "Cannot delete category because it is used by articles."
                );
            }

            var category = context.Categories
                .FirstOrDefault(c => c.CategoryId == categoryId);

            if (category == null)
                throw new Exception("Category not found.");

            context.Categories.Remove(category);
            context.SaveChanges();
        }

        // =====================================================
        // 6. TOGGLE ISACTIVE
        // =====================================================
        public void ToggleActive(FunewsManagementContext context, int? categoryId)
        {
            var category = context.Categories
                .FirstOrDefault(c => c.CategoryId == categoryId);

            if (category == null)
                throw new Exception("Category not found.");

            category.IsActive = !category.IsActive;
            context.SaveChanges();
        }


        // =====================================================
        // 8. COUNT ARTICLES PER CATEGORY (JOIN + COUNT)
        // =====================================================
        public IQueryable<CategoryArticleCount> GetCategoryArticleCounts(
            FunewsManagementContext context)
        {
            return context.Categories
                .GroupJoin(
                    context.NewsArticles,
                    c => c.CategoryId,
                    n => n.CategoryId,
                    (c, articles) => new CategoryArticleCount
                    {
                        CategoryId = c.CategoryId,
                        CategoryName = c.CategoryName,
                        CategoryDesciption = c.CategoryDescription,
                        IsActive = c.IsActive,
                        ArticleCount = articles.Count()
                    }
                );
        }

        public short GenerateNewCategoryId(FunewsManagementContext context)
        {
            int maxId = context.Tags.Any()
            ? context.Categories.Max(a => a.CategoryId)
            : 0;

            return (short)(maxId + 1);
        }
    }
    public class CategoryArticleCount
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }

        public string CategoryDesciption { get; set; }

        public bool? IsActive { get; set; }
        public int ArticleCount { get; set; }
    }
}
