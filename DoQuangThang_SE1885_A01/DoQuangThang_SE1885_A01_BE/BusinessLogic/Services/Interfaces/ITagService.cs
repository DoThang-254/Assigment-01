using BusinessLogic.Dto;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Interfaces
{
    public interface ITagService
    {
        public IEnumerable<Tag> GetByIds(List<int> ids);
        void DeleteTag(int tagId);
        IEnumerable<Tag> GetAll();

        List<Tag> GetTagsByIds(List<int> ids);
        Tag? GetById(int tagId);
        void AddTag(TagDto tag);
        void UpdateTag(TagUpdateRequestDto tag);

    }
}
