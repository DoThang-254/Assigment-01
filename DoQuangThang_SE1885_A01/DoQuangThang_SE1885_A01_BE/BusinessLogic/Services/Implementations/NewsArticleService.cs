using BusinessLogic.Dto;
using BusinessLogic.Services.Interfaces;
using DataAccess.Models;
using DataAccess.Models.Dto;
using DataAccess.Repositories.Implementations;
using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Implementations
{
    public class NewsArticleService : INewsArticleService
    {
        private readonly INewsArticleRepository _newsArticleRepository;
        private readonly ITagRepository _tagsRepository;

        public NewsArticleService(INewsArticleRepository newsArticleRepository, ITagRepository tagsRepository)
        {
            _newsArticleRepository = newsArticleRepository;
            _tagsRepository = tagsRepository;
        }

        public void AddNews(NewsDto articles)
        {

            var newArticles = new NewsArticle
            {
                NewsArticleId = Guid.NewGuid().ToString("N").Substring(0, 20),
                NewsTitle = articles.NewsTitle,
                NewsContent = articles.NewsContent,
                Headline = articles.Headline,
                NewsSource = articles.NewsSource,
                CategoryId = articles.CategoryId,
                CreatedDate = DateTime.Now,
                CreatedById = articles.CreatedById,
                NewsStatus = true,
            };

            _newsArticleRepository.AddNewsArticle(newArticles , articles.TagIds);
        }

        public IQueryable<NewsArticle> GetNewsByUserId(short? userId)
        {
            if (!userId.HasValue)
            {
                throw new Exception("User ID cannot be null.");
            }

            return _newsArticleRepository.GetNewsByUserId(userId);
        }

        public IQueryable<NewsArticle> GetAllNews()
        {
            return _newsArticleRepository.GetNewsArticles();
        }

        public int ReportNewsByCategory(int? categoryId)
        {
            return _newsArticleRepository.CountNewsByCategory(categoryId);
        }

        public void UpdateNews(NewsDto news)
        {
            var updateNews = _newsArticleRepository.GetNewsArticleById(news.NewsArticleId);
            if (updateNews == null)
            {
                throw new Exception($"News Article with ID {news.NewsArticleId} not found.");
            }
            updateNews.NewsArticleId = news.NewsArticleId;
            updateNews.NewsStatus = news.NewsStatus;
            updateNews.NewsTitle = news.NewsTitle;
            updateNews.Headline = news.Headline;
            updateNews.NewsContent = news.NewsContent;
            updateNews.NewsSource = news.NewsSource;
            updateNews.CategoryId = news.CategoryId;
            updateNews.ModifiedDate = DateTime.Now;
            updateNews.UpdatedById = news.UpdatedById;

            var newTagIds = news.TagIds ?? new List<int>();

            updateNews.Tags.Clear();

            if (news.TagIds != null && news.TagIds.Any())
            {
                var tags = _tagsRepository.GetTagsByIds(news.TagIds);
                foreach (var tag in tags)
                {
                    updateNews.Tags.Add(tag);
                }
            }

            _newsArticleRepository.UpdateNewsArticle(updateNews);
        }

        public void DeleteNews(string newsArticleId)
        {
            _newsArticleRepository.DeleteNewsArticle(newsArticleId);
        }

        public IQueryable<AuthorDto> GetAuthorDtos()
        {
           return _newsArticleRepository.GetAuthorDtos();
        }

        public NewsArticle? GetNewsById(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }
            return _newsArticleRepository.GetNewsArticleById(key);
        }
    }
}
