using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using MyORM;

namespace HttpServer.Models
{
    [DB_Table]
    public class Account
    {
        [DB_Field]
        [DB_Identity]
        public int Id { get; }

        [DB_Field]
        public string Name { get; }

        [DB_Field]
        public string Password { get; }

        public Account(string name, string password)
        {
            Name = name;
            Password = password;
        }

        private Account(int id, string name, string password)
        {
            Id = id;
            Name = name;
            Password = password;
        }
    }
}
