using System;

namespace TCPServer
{
    internal class Program
    {
        private static void Main()
        {
            Console.Title = "XServer";
            Console.ForegroundColor = ConsoleColor.White;

            XServer.Start();
            XServer.AcceptClients();
        }
    }
}
