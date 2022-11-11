using Microsoft.Extensions.Caching.Memory;
using MyORM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpServer.Session
{
    [DB_Table]
    public class Session
    {
        [DB_Field]
        [DB_Identity]
        public int Id { get; } = 0;

        [DB_Field]
        public string Guid {get; }

        public Session(string guid)
        {
            Guid = guid;
        }

        private Session(int id, string guid)
        {
            Id = id;
            Guid = guid;
        }
    }
}
