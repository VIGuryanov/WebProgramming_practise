using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpServer.Models
{
    public class Account
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }

        public Account(int? id = null, string? name = null, string? password = null)
        {
            Id = id == null? 0:(int)id;
            Name = name;
            Password = password;
        }
    }
}
