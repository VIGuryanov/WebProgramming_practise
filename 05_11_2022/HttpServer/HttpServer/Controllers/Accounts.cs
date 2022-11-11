using HttpServer.Attributes;
using HttpServer.Models;
using System.Net;
using HttpServer.SQLPatterns;
using System.Data.SqlClient;
using MyORM;

namespace HttpServer.Controllers
{
    [ApiController]
    public class Accounts
    {
        [HttpGet]
        public List<Account> GetAccounts() => new AccountRepository(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SteamDB;Integrated Security=True").GetValues();
        /*{
            var orm = new MyORM.MyORM(@"(localdb)\MSSQLLocalDB", "SteamDB", true);
            return orm.Select<Account>();
        }*/
        
        [HttpGet]
        public Account? GetAccountById(int id) => new AccountRepository(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SteamDB;Integrated Security=True").Find(id);
        /*{
            return GetAccounts().FirstOrDefault(a => a.Id == id);
        }*/

        [HttpPost]
        public void SaveAccount(string name, string password, HttpListenerResponse response)
        {
            /*var orm = new MyORM.MyORM(@"(localdb)\MSSQLLocalDB", "SteamDB", true);
            orm.Insert(new Account(0, name, password));*/
            new AccountRepository(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SteamDB;Integrated Security=True").Add(new Account(name, password));
            response.Redirect("https://store.steampowered.com/login");
        }

        [HttpPost]
        public bool Login(string name, string password)
        {
            return new AccountRepository(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SteamDB;Integrated Security=True")
                .GetValues().Where(x=>x.Name==name && x.Password==password).Any();
        }
    }
}
