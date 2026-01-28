using DataAccess.DAO;
using DataAccess.Models;
using DataAccess.Models.Dto;
using DataAccess.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories.Implementations
{
    public class NewsArticleRepository : INewsArticleRepository
    {
        private readonly FunewsManagementContext _context;

        public NewsArticleRepository(FunewsManagementContext context)
        {
            _context = context;
        }

        public void AddNewsArticle(NewsArticle article, List<int> tagIds) => NewsArticleDAO.Instance.AddNewsArticle(article, tagIds, _context);

        public int CountNewsByCategory(int? categoryId) => NewsArticleDAO.Instance.CountNewsByCategory(categoryId, _context);

        public void DeleteNewsArticle(string id) => NewsArticleDAO.Instance.DeleteNewsArticle(id, _context);

        public NewsArticle? GetNewsArticleById(string id) => NewsArticleDAO.Instance.GetNewsArticleById(id, _context);

        public IQueryable<NewsArticle> GetNewsArticles() => NewsArticleDAO.Instance.GetNewsArticles(_context);

        public IQueryable<NewsArticle> GetNewsByUserId(short? userId) => NewsArticleDAO.Instance.GetNewsArticleByUserId(userId, _context);

        public void UpdateNewsArticle(NewsArticle article) => NewsArticleDAO.Instance.UpdateNewsArticle(article, _context);

        public IQueryable<AuthorDto> GetAuthorDtos() => NewsArticleDAO.Instance.GetAuthorDtos(_context);

    }
}
