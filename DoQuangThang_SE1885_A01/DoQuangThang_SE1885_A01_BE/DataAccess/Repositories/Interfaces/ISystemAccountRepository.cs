using DataAccess.DAO;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories.Interfaces
{
    public interface ISystemAccountRepository
    {
        public IQueryable<SystemAccount> GetAccounts();
        public SystemAccount? GetAccountByEmail(string email);
        public SystemAccount? GetAccountById(int accountId);

        public IQueryable<SystemAccount> SearchAccounts(string? name , string? email , int? role);

        public void AddAccount(SystemAccount account);

        public void UpdateAccount(SystemAccount account);

        public void DeleteAccount(short id);

        public short GenerateNewAccountId();
    }
}
