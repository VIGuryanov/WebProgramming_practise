using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpServer.Session
{
    public class Session
    {
        public bool IsAuthorize { get; }
        public int Id { get; }

        public Session(bool isAuthorize, int id)
        {
            IsAuthorize = isAuthorize;
            Id = id;
        }

        public override string ToString() =>$"IsAuthorize:{IsAuthorize} Id={Id}";
    }
}
