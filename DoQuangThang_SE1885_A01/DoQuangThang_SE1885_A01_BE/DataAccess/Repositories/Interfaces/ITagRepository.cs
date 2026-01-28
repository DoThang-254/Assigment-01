using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories.Interfaces
{
    public interface ITagRepository
    {
        public IEnumerable<Tag> GetByIds(List<int> ids);
        void DeleteTag(int tagId);

        IEnumerable<Tag> GetAll();
        List<Tag> GetTagsByIds(List<int> ids);
        Tag? GetById(int tagId);
        void AddTag(Tag tag);
        void UpdateTag(Tag tag);
        bool IsTagUsed(int tagId);

        public int GenerateNewTagId();
    }
}
