using DataAccess.DAO;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;

namespace DataAccess.Repository
{
    public class SystemAccountRepository : ISystemAccountRepository
    {
        private readonly FunewsManagementContext _context;

        public SystemAccountRepository(FunewsManagementContext context)
        {
            _context = context;
        }

        public IQueryable<SystemAccount> GetAccounts()
        {
            return SystemAccountDAO.Instance.GetAccounts(_context);
        }

        public SystemAccount? GetAccountById(int id)
        {
            return SystemAccountDAO.Instance.GetAccountById(id, _context);
        }

        public void AddAccount(SystemAccount account) => SystemAccountDAO.Instance.AddAccount(account, _context);

        public void UpdateAccount(SystemAccount account) => SystemAccountDAO.Instance.UpdateAccount(account, _context);

        public void DeleteAccount(short id) => SystemAccountDAO.Instance.DeleteAccount(id, _context);

        public SystemAccount? GetAccountByEmail(string email)
        {
            return SystemAccountDAO.Instance.GetAccountByEmail(email, _context);
        }
        public IQueryable<SystemAccount> SearchAccounts(string? name, string? email, int? role)
        {
            return SystemAccountDAO.Instance.SearchAccounts(name, email, role, _context);
        }

        public short GenerateNewAccountId() => SystemAccountDAO.Instance.GenerateNewAccountId(_context);

        public SystemAccount? GetLastEditorByNewsArticleId(string newsArticleId)
        => SystemAccountDAO.Instance.GetLastEditorByNewsArticleId(newsArticleId, _context);

        public bool CheckExistEmail(string email) => SystemAccountDAO.Instance.CheckExistEmail(email, _context);

    }
}