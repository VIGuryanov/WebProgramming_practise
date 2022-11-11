using HttpServer.Attributes;
using HttpServer.Models;
using System.Net;
using HttpServer.SQLPatterns;
using System.Data.SqlClient;
using MyORM;
using static Microsoft.AspNetCore.Hosting.Internal.HostingApplication;
using System.Text.Json.Nodes;
using HttpServer.Session;

namespace HttpServer.Controllers
{
    [ApiController]
    public class Accounts
    {
        [HttpGet]
        public List<Account> GetAccounts(HttpListenerContext context)
        {
            if(CheckSession(context))
                return new AccountRepository(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SteamDB;Integrated Security=True").GetValues();             
            context.Response.StatusCode = 401;
            return new List<Account>();
        }

        [HttpGet]
        public Account? GetAccountById(int id, HttpListenerContext context)
        {
            if(CheckSession(context))
                return new AccountRepository(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SteamDB;Integrated Security=True").Find(id);
            context.Response.StatusCode = 401;
            return null;
        }

        [HttpGet]
        public Account? GetAccountInfo(HttpListenerContext context)
        {
            throw new InvalidOperationException();
            /*if(CheckSession(context, out Account? acc))
                return acc;
            context.Response.StatusCode = 401;
            return acc;*/
        }

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
                .GetValues().Where(x => x.Name == name && x.Password == password);

            if (dbContent.Any())
            {
                var account = dbContent.First();
                var cookie = new Cookie("SessionId", Guid.NewGuid().ToString(), "/")
                {
                    Expires = DateTime.Now + new TimeSpan(20 * TimeSpan.TicksPerMinute)
                };
                context.Response.AppendCookie(cookie);
                SessionManager.CreateSession(new Session.Session(cookie.Value));
            }
            context.Response.Redirect("/");
        }

        private bool CheckSession(HttpListenerContext context)
        { 
            var sessionCookie = context.Request.Cookies.Where(x => x.Name == "SessionId").FirstOrDefault();
            if (sessionCookie != null)
            {
                var session = new Session.Session(sessionCookie.Value);
                if(SessionManager.CheckSession(session))
                    return true;
            }
            return false;
        }
    }
}
