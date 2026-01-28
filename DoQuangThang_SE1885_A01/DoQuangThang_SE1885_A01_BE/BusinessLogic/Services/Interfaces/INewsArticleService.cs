using BusinessLogic.Dto;
using DataAccess.Models;
using DataAccess.Models.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Interfaces
{
    public interface INewsArticleService
    {
        IQueryable<NewsArticle> GetAllNews();

        int ReportNewsByCategory(int? categoryId);

        void AddNews(NewsDto articles);

        void UpdateNews(NewsDto articles); 

        void DeleteNews(string id);

        public IQueryable<NewsArticle> GetNewsByUserId(short? userId);

        IQueryable<AuthorDto> GetAuthorDtos();

        NewsArticle? GetNewsById(string key);

    }
}
