using DataAccess.Models;
using DataAccess.Models.Dto;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DAO
{
    public class NewsArticleDAO
    {
        private static NewsArticleDAO? instance = null;
        private static readonly object instanceLock = new object();

        private NewsArticleDAO()
        {
        }

        public static NewsArticleDAO Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new NewsArticleDAO();
                    }

                    return instance;
                }
            }
        }

        public IQueryable<NewsArticle> GetNewsArticles(FunewsManagementContext context)
        {
            return context.NewsArticles;
        }

        public NewsArticle? GetNewsArticleById(string id, FunewsManagementContext context)
        {

            return context.NewsArticles.Include(n => n.Tags).FirstOrDefault(c => c.NewsArticleId == id);

        }

        public IQueryable<NewsArticle> GetNewsArticleByUserId(short? userId, FunewsManagementContext context)
        {

            return context.NewsArticles.Where(n => n.CreatedById == userId);

        }

        public void AddNewsArticle(NewsArticle article, List<int> tagIds, FunewsManagementContext context)
        {
            // 1. Khởi tạo list Tags trong bài viết nếu null
            if (article.Tags == null)
            {
                article.Tags = new List<Tag>();
            }

            // 2. Xử lý gán Tags (Quan trọng nhất)
            if (tagIds != null && tagIds.Any())
            {
                foreach (var id in tagIds)
                {
                    // Kiểm tra xem Tag này có đang được Context theo dõi không (tránh lỗi Attach trùng)
                    var tag = context.Tags.FirstOrDefault(t => t.TagId == id);

                    if (tag == null)
                    {
                        // Nếu chưa có, tạo một object giả (Stub) chỉ chứa ID
                        tag = new Tag { TagId = id };
                        // Báo cho EF biết Tag này ĐÃ TỒN TẠI dưới DB, đừng tạo mới
                        context.Tags.Attach(tag);
                    }

                    // Thêm Tag vào danh sách của bài viết
                    article.Tags.Add(tag);
                }
            }

            // 3. Thêm bài viết vào Context
            // EF sẽ tự động hiểu bài viết là Mới (Add), còn các Tags bên trong là Cũ (do đã Attach)
            // => Nó sẽ chỉ sinh ra lệnh INSERT vào bảng NewsArticle và bảng trung gian NewsTag
            context.NewsArticles.Add(article);

            // 4. Lưu thay đổi
            context.SaveChanges();
        }
        public void UpdateNewsArticle(NewsArticle article, FunewsManagementContext context)
        {

            context.Entry<NewsArticle>(article).State = EntityState.Modified;
            context.SaveChanges();
        }

        public void DeleteNewsArticle(string id, FunewsManagementContext context)
        {

            var p = context.NewsArticles.Include(n => n.Tags).SingleOrDefault(c => c.NewsArticleId == id);
            if (p != null)
            {
                p.Tags.Clear();   
                context.NewsArticles.Remove(p);
                context.SaveChanges();
            }
        }


        public int CountNewsByCategory(int? categoryId, FunewsManagementContext context)
        {
            return context.NewsArticles.Count(n => n.CategoryId == categoryId);
        }

        public IQueryable<AuthorDto> GetAuthorDtos(FunewsManagementContext context)
        {
            return context.NewsArticles
                .Where(n => n.CreatedBy != null)
                .Select(n => new AuthorDto
                {
                    AuthorId = n.CreatedById,
                    AuthorName = n.CreatedBy.AccountName
                })
                .Distinct();
        }


    }
}
