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
    public class TagRepository : ITagRepository
    {
        private readonly FunewsManagementContext _context;

        public TagRepository(FunewsManagementContext context)
        {
            _context = context;
        }

        public IEnumerable<Tag> GetAll() => TagDAO.Instance.GetAll(_context);

        public void DeleteTag(int tagId) => TagDAO.Instance.DeleteTag(tagId , _context);

        public IEnumerable<Tag> GetByIds(List<int> ids) => TagDAO.Instance.GetByIds(ids , _context);

        public List<Tag> GetTagsByIds(List<int> ids) => TagDAO.Instance.GetTagsByIds(ids , _context);

        public Tag? GetById(int tagId) => TagDAO.Instance.GetById(tagId , _context);

        public void AddTag(Tag tag) => TagDAO.Instance.AddTag(tag , _context);

        public void UpdateTag(Tag tag) => TagDAO.Instance.UpdateTag(tag , _context);

        public bool IsTagUsed(int tagId) => TagDAO.Instance.IsTagUsed(tagId , _context);

        public int GenerateNewTagId() => TagDAO.Instance.GenerateNewTagId(_context);
    }
}
