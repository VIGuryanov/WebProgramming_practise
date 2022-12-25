using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using XProtocol;
using XProtocol.Packets;
using XProtocol.Serializator;

namespace TCPServer
{
    internal class ConnectedClient
    {
        public Socket Client { get; }
        string nick;
        NickNamePacket encodedNick;

        Color color = Color.FromArgb(new Random().Next(255), new Random().Next(255), new Random().Next(255));

        private readonly Queue<byte[]> _packetSendingQueue = new Queue<byte[]>();

        public ConnectedClient(Socket client)
        {
            Client = client;

            Task.Run((Action)ProcessIncomingPackets);
            Task.Run((Action)SendPackets);
        }

        private void ProcessIncomingPackets()
        {
            try
            {
                while (true) // Слушаем пакеты, пока клиент не отключится.
                {
                    var buff = new byte[256]; // Максимальный размер пакета - 256 байт.
                    Client.Receive(buff);

                    buff = buff.TakeWhile((b, i) =>
                    {
                        if (b != 0xFF) return true;
                        return buff[i + 1] != 0;
                    }).Concat(new byte[] { 0xFF, 0 }).ToArray();

                    var parsed = XPacket.Parse(buff);

                    if (parsed != null)
                    {
                        ProcessIncomingPacket(parsed);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                XServer._clients.Remove(this);
            }
        }

        private void ProcessIncomingPacket(XPacket packet)
        {
            var type = XPacketTypeManager.GetTypeFromPacket(packet);

            switch (type)
            {
                case XPacketType.Handshake:
                    break;
                case XPacketType.Unknown:
                    break;
                case XPacketType.User:
                    ProcessUser(packet);
                    break;
                case XPacketType.ColoredPoint:
                    ProcessColoredPoint(packet);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ProcessUser(XPacket packet)
        {
            var user = XPacketConverter.Deserialize<UserPacket>(packet);

            nick = user.NickName.DecodeToString();
            encodedNick = NickNamePacket.EncodeString(nick);

            foreach (var client in XServer._clients)
                client.QueuePacketSend(XPacketConverter.Serialize(XPacketType.User,
                    new UserPacket { NickName = encodedNick, Color = color.ToArgb() }).ToPacket());
        }

        private void ProcessColoredPoint(XPacket packet)
        {
            var point = XPacketConverter.Deserialize<ColoredPoint>(packet);

            foreach (var client in XServer._clients)
                client.QueuePacketSend(XPacketConverter.Serialize(XPacketType.ColoredPoint,
                    new ColoredPoint { Color = color.ToArgb(), X = point.X, Y = point.Y }).ToPacket());
        }

        public void QueuePacketSend(byte[] packet)
        {
            if (packet.Length > 256)
            {
                throw new Exception("Max packet size is 256 bytes.");
            }

            _packetSendingQueue.Enqueue(packet);
        }

        private void SendPackets()
        {
            try
            {
                while (true)
                {
                    if (_packetSendingQueue.Count == 0)
                    {
                        Thread.Sleep(100);
                        continue;
                    }

                    var packet = _packetSendingQueue.Dequeue();
                    Client.Send(packet);

                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                XServer._clients.Remove(this);
            }
        }
    }
}
