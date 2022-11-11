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
        public List<Account> GetAccounts(HttpListenerContext context)
        {
            var getSessionCookie = context.Request.Cookies.Where(x=>x.Name == "SessionId" && x.Value.Contains("IsAuthorize:true"));
            if(getSessionCookie.Any())
                return new AccountRepository(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SteamDB;Integrated Security=True").GetValues();
            context.Response.StatusCode = 401;
            return new List<Account>();
        }
        
        [HttpGet]
        public Account? GetAccountById(int id) => new AccountRepository(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SteamDB;Integrated Security=True").Find(id);
        /*{
            return GetAccounts().FirstOrDefault(a => a.Id == id);
        }*/

        [HttpPost]
        public void SaveAccount(string name, string password, HttpListenerContext context)
        {
            /*var orm = new MyORM.MyORM(@"(localdb)\MSSQLLocalDB", "SteamDB", true);
            orm.Insert(new Account(0, name, password));*/
            new AccountRepository(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SteamDB;Integrated Security=True").Add(new Account(name, password));
            context.Response.Redirect("https://store.steampowered.com/login");
        }

        [HttpPost]
        public void Login(string name, string password, HttpListenerContext context)
        {
            var dbContent = new AccountRepository(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SteamDB;Integrated Security=True")
                .GetValues().Where(x=>x.Name==name && x.Password==password);
            if(dbContent.Any())
            {
                var cookie = new Cookie("SessionId",$"IsAuthorize:true Id={dbContent.First().Id}","/");
                cookie.Expires = DateTime.Now + new TimeSpan(2*TimeSpan.TicksPerMinute);
                context.Response.AppendCookie(cookie);
            }
            context.Response.Redirect("/");
        }
    }
}
