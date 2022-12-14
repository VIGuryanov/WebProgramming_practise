using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace TCPServer
{
    internal static class XServer
    {
        private static readonly Socket _socket;
        internal static readonly List<ConnectedClient> _clients;

        private static bool _listening;
        private static bool _stopListening;

        static XServer()
        {
            var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());  
            var ipAddress = ipHostInfo.AddressList[1];

            _socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _clients = new List<ConnectedClient>();
        }

        public static void Start()
        {
            if (_listening)
            {
                throw new Exception("Server is already listening incoming requests.");
            }

            _socket.Bind(new IPEndPoint(IPAddress.Any, 4910));
            _socket.Listen(10);

            _listening = true;
        }

        public static void Stop()
        {
            if (!_listening)
            {
                throw new Exception("Server is already not listening incoming requests.");
            }

            _stopListening = true;
            _socket.Shutdown(SocketShutdown.Both);
            _listening = false;
        }

        public static void AcceptClients()
        {
            while (true)
            {
                if (_stopListening)
                {
                    return;
                }

                Socket client;

                try
                {
                    client = _socket.Accept();
                } catch { return; }

                Console.WriteLine($"[!] Accepted client from {(IPEndPoint) client.RemoteEndPoint}");

                var c = new ConnectedClient(client);
                _clients.Add(c);
            }
        }
    }
}
