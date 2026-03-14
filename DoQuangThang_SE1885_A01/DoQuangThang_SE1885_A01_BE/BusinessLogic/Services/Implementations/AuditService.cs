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
    public class AuditService : IAuditService
    {
        private readonly IAuditRepository _repo;
        public AuditService(IAuditRepository repo) => _repo = repo;

        public Task<List<AuditLog>> GetLogs(string? email, string? entityName)
        {
            return _repo.GetAuditLogsAsync(email, entityName);
        }
    }
}
