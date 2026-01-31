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
            return context.NewsArticles.Include(n => n.CreatedBy).Include(n => n.Category);
        }

        public NewsArticle? GetNewsArticleById(string id, FunewsManagementContext context)
        {

            return context.NewsArticles.Include(n => n.Tags).Include(n=>n.CreatedBy).Include(n => n.Category).FirstOrDefault(c => c.NewsArticleId == id);

        }

        public IQueryable<NewsArticle> GetNewsArticleByUserId(short? userId, FunewsManagementContext context)
        {

            return context.NewsArticles.Where(n => n.CreatedById == userId);

        }

        public void AddNewsArticle(NewsArticle article, List<int> tagIds, FunewsManagementContext context)
        {
            if (article.Tags == null)
            {
                article.Tags = new List<Tag>();
            }

            if (tagIds != null && tagIds.Any())
            {
                foreach (var id in tagIds)
                {
                    var tag = context.Tags.FirstOrDefault(t => t.TagId == id);

                    if (tag == null)
                    {
                        tag = new Tag { TagId = id };
                        context.Tags.Attach(tag);
                    }

                    article.Tags.Add(tag);
                }
            }

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
