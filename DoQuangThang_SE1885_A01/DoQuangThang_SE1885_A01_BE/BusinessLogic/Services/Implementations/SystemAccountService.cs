using BusinessLogic.Services.Interfaces;
using DataAccess.Constants;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.Options;
using Presentation.ViewModels.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Implementations
{
    public class SystemAccountService : ISystemAccountService
    {
        private readonly ISystemAccountRepository _systemAccountRepository;
        private readonly INewsArticleRepository _newsArticleRepository;
        private readonly AdminAccount _admin;


        public SystemAccountService(ISystemAccountRepository systemAccountRepository,
        IOptions<AdminAccount> options, INewsArticleRepository newsArticleRepository
        )
        {
            _systemAccountRepository = systemAccountRepository;
            _newsArticleRepository = newsArticleRepository;
            _admin = options.Value;
        }
        public void AddAccount(CreateAccountRequest newAccount)
        {
            var exist = _systemAccountRepository.GetAccountByEmail(newAccount.AccountEmail);
            if (exist != null)
            {
                throw new Exception("Email already exists!");
            }
            short newId = _systemAccountRepository.GenerateNewAccountId();

            var account = new SystemAccount
            {
                AccountId = newId,
                AccountName = newAccount.AccountName,
                AccountEmail = newAccount.AccountEmail,
                AccountPassword = newAccount.AccountPassword,
                AccountRole = newAccount.AccountRole,
            };
            _systemAccountRepository.AddAccount(account);
        }

        public SystemAccount? GetAccountById(int? id)
        {
            if (!id.HasValue)
            {
                throw new Exception("Account ID is required");
            }

            return _systemAccountRepository.GetAccountById(id.Value);
        }

        public SystemAccount? Login(string email, string password)
        {
            var account = _systemAccountRepository.GetAccountByEmail(email);
            if (email == _admin.Email && password == _admin.Password)
            {
                return new SystemAccount
                {
                    AccountId = 0,
                    AccountEmail = _admin.Email,
                    AccountRole = Roles.Admin,
                };
            }
            if (account != null && account.AccountPassword == password)
            {
                return account;
            }
            return null;
        }


        public IQueryable<SystemAccount> GetAccounts()
        {
            return _systemAccountRepository.GetAccounts();
        }


        public void UpdateAccount(SystemAccount account)
        {
            _systemAccountRepository.UpdateAccount(account);
        }

        public bool DeleteAccount(short accountId)
        {
            var articles = _newsArticleRepository.GetNewsArticles();
            bool hasArticles = articles.Any(n => n.CreatedById == accountId);

            if (hasArticles)
            {
                return false;
            }

            _systemAccountRepository.DeleteAccount(accountId);
            return true;
        }

        public IQueryable<SystemAccount> Search(string? name, string? email, int? role)
        {
            return _systemAccountRepository.SearchAccounts(name, email, role);

        }

        public void ChangePassword(int accountId, string oldPassword, string newPassword)
        {
            var account = _systemAccountRepository.GetAccountById(accountId);
            if (account == null)
                throw new Exception("Account not found");

            if (account.AccountPassword != oldPassword) 
            {
                throw new Exception("Old password is incorrect");
            }

            account.AccountPassword = newPassword;
            _systemAccountRepository.UpdateAccount(account);
        }

        public void AdminResetPassword(string adminEmail, string adminPassword, int targetAccountId, string newPassword)
        {
            var adminAccount = _admin.Email.Equals(adminEmail);

            if (!adminAccount)
                throw new Exception("Unauthorized: Only Admin can perform this action");

            if (_admin.Password != adminPassword)
                throw new Exception("Admin password incorrect. Verification failed.");

            var targetAccount = _systemAccountRepository.GetAccountById(targetAccountId);
            if (targetAccount == null)
                throw new Exception("Target account not found");

            targetAccount.AccountPassword = newPassword;
            _systemAccountRepository.UpdateAccount(targetAccount);
        }
    }
}
