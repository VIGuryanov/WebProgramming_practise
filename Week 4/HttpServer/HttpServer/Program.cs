using System;
using System.Net;
using System.IO;
 
namespace HttpServer
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                var input = Console.ReadLine();                
                if (input == "Start")
                {
                    if(!HTTPServer.server.IsAlive)
                        HTTPServer.server.Start();
                    else
                        Console.WriteLine("Double run unsupported");
                }
                else if (input == "Stop")
                {
                    if(HTTPServer.IsRun)
                        HTTPServer.Stop();
                    else
                        Console.WriteLine("Server not running yet");
                }
                else
                    Console.WriteLine("Unsupported command");
            }
        }
    }
}