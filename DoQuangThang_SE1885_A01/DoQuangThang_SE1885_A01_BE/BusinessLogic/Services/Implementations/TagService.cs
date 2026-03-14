using BusinessLogic.Dto;
using BusinessLogic.Services.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Implementations
{
    public class TagService : ITagService
    {
        private readonly ITagRepository _tagRepository;

        public TagService(ITagRepository tagRepository)
        {
            _tagRepository = tagRepository;
        }

        // CREATE
        public void AddTag(TagDto tag)
        {
            if (string.IsNullOrWhiteSpace(tag.TagName))
                throw new Exception("Tag name is required.");
            var newId = _tagRepository.GenerateNewTagId();
            var newTag = new Tag
            {
                TagId = newId,
                TagName = tag.TagName,
                Note = tag.Note
            };
            _tagRepository.AddTag(newTag);
        }

        // DELETE (không cho xóa nếu đang được dùng)
        public void DeleteTag(int tagId)
        {
            var tag = _tagRepository.GetById(tagId);
            if (tag == null)
                throw new Exception("Tag not found.");

            bool isUsed = _tagRepository.IsTagUsed(tagId);
            if (isUsed)
                throw new Exception("Cannot delete tag because it is referenced in news articles.");

            _tagRepository.DeleteTag(tagId);
        }

        // READ ALL (OData sẽ filter/search)
        public IEnumerable<Tag> GetAll()
        {
            return _tagRepository.GetAll();
        }

        // READ BY ID
        public Tag? GetById(int tagId)
        {
            return _tagRepository.GetById(tagId);
        }

        // Dùng nội bộ (VD: update news)
        public IEnumerable<Tag> GetByIds(List<int> ids)
        {
            if (ids == null || !ids.Any())
                return Enumerable.Empty<Tag>();

            return _tagRepository.GetByIds(ids);
        }

        public IQueryable<NewsArticle> GetNewsArticlesByTagId(int tagId)
        {
            var res = _tagRepository.GetNewsArticlesByTagId(tagId);
            if(res == null)
                throw new Exception("Tag not found.");
            return res;
        }

        // Alias cho GetByIds (nếu bạn đang dùng chỗ khác)
        public List<Tag> GetTagsByIds(List<int> ids)
        {
            if (ids == null || !ids.Any())
                return new List<Tag>();

            return _tagRepository.GetTagsByIds(ids);
        }

        // UPDATE (Edit Note / TagName)
        public void UpdateTag(TagUpdateRequestDto tag)
        {
            var existing = _tagRepository.GetById(tag.TagId);
            if (existing == null)
                throw new Exception("Tag not found.");

            existing.TagName = tag.TagName;
            existing.Note = tag.Note;

            _tagRepository.UpdateTag(existing);
        }
    }
}
