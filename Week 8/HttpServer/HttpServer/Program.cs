using System;
using System.Net;
using System.IO;

namespace HttpServer
{
    class Program
    {
        static void Main()
        {
            var server = new HTTPServer();
            while (true)
            {
                var input = Console.ReadLine();
                if (input == "Start")
                    HTTPServer.ServerCommands.Start(server);
                else if (input == "Stop")
                    HTTPServer.ServerCommands.Stop(server);
                else
                    Console.WriteLine("Unsupported command");
            }
        }
    }
}