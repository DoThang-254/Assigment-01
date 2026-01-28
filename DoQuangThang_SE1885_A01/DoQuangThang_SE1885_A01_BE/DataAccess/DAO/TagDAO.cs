using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DAO
{
    public class TagDAO
    {
        private static TagDAO? instance = null;
        private static readonly object instanceLock = new object();

        private TagDAO()
        {
        }

        public static TagDAO Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new TagDAO();
                    }

                    return instance;
                }
            }
        }

        public IEnumerable<Tag> GetAll(FunewsManagementContext context)
        {
            return context.Tags;
        }

        public IEnumerable<Tag> GetByIds(List<int> ids , FunewsManagementContext context)
        {
            return context.Tags.Where(t => ids.Contains(t.TagId));
        }

        public Tag? GetById(int tagId, FunewsManagementContext context)
        {
            return context.Tags.FirstOrDefault(t => t.TagId == tagId);
        }

        public void AddTag(Tag tag, FunewsManagementContext context)
        {
            context.Tags.Add(tag);
            context.SaveChanges();
        }


        public void UpdateTag(Tag tag, FunewsManagementContext context)
        {
            context.Tags.Update(tag);
            context.SaveChanges();
        }

        public List<Tag> GetTagsByIds(List<int> ids , FunewsManagementContext context)
        {
            return context.Tags
                .Where(t => ids.Contains(t.TagId))
                .ToList();
        }

        public void DeleteTag(int tagId, FunewsManagementContext context)
        {
            bool isUsed = context.NewsArticles
                .Any(n => n.Tags.Any(t => t.TagId == tagId));

            if (isUsed)
                throw new Exception("Cannot delete tag because it is used in news articles.");

            var tag = context.Tags.Find(tagId);
            if (tag == null)
                throw new Exception("Tag not found.");

            context.Tags.Remove(tag);
            context.SaveChanges();
        }


        public bool IsTagUsed(int tagId , FunewsManagementContext context)
        {
            return context.NewsArticles
                .Any(n => n.Tags.Any(t => t.TagId == tagId));
        }

        public int GenerateNewTagId(FunewsManagementContext context)
        {
            int maxId = context.Tags.Any()
            ? context.Tags.Max(a => a.TagId)
            : 0;

            return (short)(maxId + 1);
        }

    }
}
