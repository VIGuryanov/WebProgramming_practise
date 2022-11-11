using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpServer.Session
{
    public static class SessionManager
    {
        static readonly MyORM.MyORM orm = new(@"(localdb)\MSSQLLocalDB", "SteamDB", true);
        static readonly SessionCache cache = new();

        public static void CreateSession(Session session)
        {
            orm.Insert(session);
            cache.GetOrCreate(session);
        }

        public static bool CheckSession(Session session) => cache.GetOrCreate(session) != null;

        public static Session? GetSessionInfo(Session session) => cache.GetOrCreate(session);
            
    }
}
