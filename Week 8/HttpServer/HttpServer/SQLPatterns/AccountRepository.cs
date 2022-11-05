using MyORM.Patterns;
using HttpServer.Models;

namespace HttpServer.SQLPatterns
{
    public class AccountRepository
    {
        readonly AccountDAO accountDAO;

        public AccountRepository(string connectionString)
        {
            accountDAO = new AccountDAO(connectionString);
        }

        public List<Account> GetValues() => accountDAO.Select();

        public void Add(Account entity) => accountDAO.Insert(entity);

        public void Remove(Account entity) => accountDAO.Delete(entity.Id);

        public Account? Find(int id) => accountDAO.Select(id);
    }
}
