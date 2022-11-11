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
        public bool IsAuthorize { get; }

        [DB_Field]
        [DB_Identity]
        public int Id { get; } = 0;

        [DB_Field]
        public int AccountId { get; }

        [DB_Field]
        public string Email { get; }

        [DB_Field]
        public DateTime createDateTime { get; }

        public Session(bool isAuthorize, int accId)
        {
            IsAuthorize = isAuthorize;
            AccountId = accId;
        }

        public Session(bool isAuthorize, int accId, string email, DateTime dt)
        {
            IsAuthorize = isAuthorize;
            AccountId = accId;
            Email = email;
            createDateTime = dt;
        }

        private Session(int id, int accId, string email, DateTime dateTime)
        {
            IsAuthorize = true;
            Id = id;
            AccountId = accId;
            Email = email;
            createDateTime = dateTime;
        }

        public override string ToString() => $"IsAuthorize:{IsAuthorize} AccountId={AccountId}";
    }
}
