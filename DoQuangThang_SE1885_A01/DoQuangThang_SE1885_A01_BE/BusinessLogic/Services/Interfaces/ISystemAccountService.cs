using BusinessLogic.Dto;
using DataAccess.Models;
using Presentation.ViewModels.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Interfaces
{
    public interface ISystemAccountService
    {
        public SystemAccount Login(string email, string password);
        public IQueryable<SystemAccount> Search(string? name , string? email , int? role);
        public void ChangePassword(int accountId, string oldPassword, string newPassword);
        public void AdminResetPassword(string adminEmail, string adminPassword, int targetAccountId, string newPassword);

        public IQueryable<SystemAccount> GetAccounts();
        public SystemAccount? GetAccountById(int? id);
        public void AddAccount(CreateAccountRequest account);

        public void UpdateAccount(short key , UpdateProfileRequestDto account);

        public bool DeleteAccount(short id);

        SystemAccount? GetLastEditorByNewsArticleId(string newsArticleId);

    }
}
