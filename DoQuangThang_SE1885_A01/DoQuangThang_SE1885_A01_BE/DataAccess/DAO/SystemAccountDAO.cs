using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace DataAccess.DAO
{
    public class SystemAccountDAO
    {
        private static SystemAccountDAO? instance = null;
        private static readonly object instanceLock = new object();

        private SystemAccountDAO() { } 

        public static SystemAccountDAO Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new SystemAccountDAO();
                    }
                    return instance;
                }
            }
        }

       
        public IQueryable<SystemAccount> GetAccounts(FunewsManagementContext context)
        {
            return context.SystemAccounts;
        }

        public SystemAccount? GetAccountById(int id, FunewsManagementContext context)
        {
            return context.SystemAccounts.FirstOrDefault(c => c.AccountId == id);
        }

        public SystemAccount? GetAccountByEmail(string email , FunewsManagementContext context)
        {
            return context.SystemAccounts.FirstOrDefault(acc => acc.AccountEmail == email);
        }


        public void AddAccount(SystemAccount account, FunewsManagementContext context)
        {
            context.SystemAccounts.Add(account);
            context.SaveChanges();
        }

        public void UpdateAccount(SystemAccount account, FunewsManagementContext context)
        {
            context.SystemAccounts.Update(account);
            context.SaveChanges();
        }

        public void DeleteAccount(short id, FunewsManagementContext context)
        {
            var acc = context.SystemAccounts.FirstOrDefault(c => c.AccountId == id);
            if (acc != null)
            {
                context.SystemAccounts.Remove(acc);
                context.SaveChanges();
            }
        }

        public IQueryable<SystemAccount> SearchAccounts(string? name, string? email, int? role, FunewsManagementContext context)
        {
            IQueryable<SystemAccount> accounts = context.SystemAccounts;
            if (!string.IsNullOrWhiteSpace(name))
            {
                accounts = accounts.Where(acc => acc.AccountName.Contains(name));
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                accounts = accounts.Where(acc =>
                    acc.AccountEmail != null &&
                    acc.AccountEmail.Contains(email));
            }

            if (role.HasValue)
            {
                accounts = accounts.Where(acc =>
                    acc.AccountRole == role.Value);
            }


            return accounts;
        }

        public short GenerateNewAccountId(FunewsManagementContext context)
        {
            int maxId = context.SystemAccounts.Any()
            ? context.SystemAccounts.Max(a => (int)a.AccountId)
            : 0;

            return (short)(maxId + 1);
        }

    }
}