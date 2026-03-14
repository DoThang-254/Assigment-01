using DataAccess.DAO;
using DataAccess.Models;
using DataAccess.Models.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories.Interfaces
{
    public interface INewsArticleRepository
    {
        public IQueryable<NewsArticle> GetNewsArticles();
        public NewsArticle? GetNewsArticleById(string id);
        public void AddNewsArticle(NewsArticle article , List<int> tagIds);
        public void UpdateNewsArticle(NewsArticle article);
        public void DeleteNewsArticle(string id);

        public int CountNewsByCategory(int? categoryId);

        public IQueryable<NewsArticle> GetNewsByUserId(short? userId);

        public IQueryable<AuthorDto> GetAuthorDtos();

    }
}
