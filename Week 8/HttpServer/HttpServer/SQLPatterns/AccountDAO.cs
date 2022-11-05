using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HttpServer.Controllers;
using HttpServer.Models;

namespace MyORM.Patterns
{
    public class AccountDAO
    {
        readonly string connectionString;
        readonly static ConstructorInfo? accountDBConstructor = typeof(Account).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, new Type[] { typeof(int), typeof(string), typeof(string) });

        public AccountDAO(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public List<Account> Select()
        {
            if(accountDBConstructor == null)
                throw new NotImplementedException("No private constructor for DB object found");
            List<Account> accounts = new();
            using (SqlConnection connection = new(connectionString))
            {
                connection.Open();
                SqlCommand command = new("SELECT * FROM [dbo].[Account]", connection);
                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        if (accountDBConstructor.Invoke(new object[] { reader.GetInt32(0), reader.GetString(1), reader.GetString(2) }) is not Account account)
                            throw new ArgumentNullException("DB object constructor returned null");
                        accounts.Add(account);
                    }
                }
                reader.Close();
            }
            return accounts;
        }

        public Account? Select(int id) => Select().FirstOrDefault(account => account.Id == id);

        public bool Delete(int id)
        {
            try
            {
                using SqlConnection connection = new SqlConnection(connectionString);
                connection.Open();
                SqlCommand command = new($"DELETE FROM [dbo].[Account] WHERE Id = {id};");
                command.ExecuteNonQuery();
                return true;
            }
            catch (SqlException)
            {
                return false;
            }
        }

        public bool Insert(Account entity)
        {
            try
            {
                using SqlConnection connection = new(connectionString);
                connection.Open();
                SqlCommand command = new($"INSERT INTO [dbo].[Account](Name, Password) VALUES('{entity.Name}', '{entity.Password}')", connection);
                command.ExecuteNonQuery();
                return true;
            }
            catch (SqlException)
            {
                return false;
            }
        }
    }
}
