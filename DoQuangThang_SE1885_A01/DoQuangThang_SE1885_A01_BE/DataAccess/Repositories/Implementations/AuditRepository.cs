using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories.Implementations
{
    public class AuditRepository : IAuditRepository
    {
        private readonly FunewsManagementContext _context;
        public AuditRepository(FunewsManagementContext context) => _context = context;

        public async Task<List<AuditLog>> GetAuditLogsAsync(string? email, string? entityName)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (!string.IsNullOrEmpty(email))
                query = query.Where(x => x.UserId.Contains(email));

            if (!string.IsNullOrEmpty(entityName))
                query = query.Where(x => x.TableName.Contains(entityName));

            return await query.OrderByDescending(x => x.Timestamp).ToListAsync();
        }
    }
}
