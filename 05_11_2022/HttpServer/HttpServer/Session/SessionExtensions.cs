using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HttpServer.Session
{
    public static class SessionExtensions
    {
        public static T Deserialize<T>(this string line) where T : Session
        {
            var isAuthorize = Regex.Match(line, "IsAuthorize:(true|false|True|False)");
            var id = Regex.Match(line, @"Id=(\d\d*)");
            if (isAuthorize.Success && id.Success)
                return (T)new Session(bool.Parse(isAuthorize.Value.Split(':')[1]), int.Parse(id.Value.Split('=')[1]));
            throw new InvalidOperationException("Failed to deserialize object");
        }
    }
}
