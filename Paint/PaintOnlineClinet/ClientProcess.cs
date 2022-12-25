using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCPClient;
using XProtocol;
using XProtocol.Packets;
using XProtocol.Serializator;

namespace PaintOnlineClient
{
    internal class ClientProcess
    {
        Form1 form;
        internal XClient Client { get; }

        internal ClientProcess(Form1 winform)
        {
            form = winform;

            Client = new XClient();
            Client.OnPacketRecieve += OnPacketRecieve;
            Client.Connect("127.0.0.1", 4910);
        }

        private void OnPacketRecieve(byte[] packet)
        {
            var parsed = XPacket.Parse(packet);

            if (parsed != null)
            {
                ProcessIncomingPacket(parsed);
            }
        }

        internal void ProcessIncomingPacket(XPacket packet)
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

        void ProcessUser(XPacket packet)
        {
            var user = XPacketConverter.Deserialize<UserPacket>(packet);

            form.AddPlayer(user.NickName.DecodeToString(), user.Color);
        }

        void ProcessColoredPoint(XPacket packet)
        {
            var point = XPacketConverter.Deserialize<ColoredPoint>(packet);

            form.Draw(new Point(point.X,point.Y), Color.FromArgb(point.Color));
        }
    }
}
