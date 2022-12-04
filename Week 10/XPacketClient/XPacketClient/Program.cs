using MyProtocol;
using System.Net.Http;
using System.Net.Sockets;

namespace XPacketClient
{
    internal class Program
    {
        public static int HandshakeMagic;
        static async Task Main(string[] args)
        {
            var client = new HttpClient();

            var rand = new Random();
            HandshakeMagic = rand.Next();

            var outStream = new MemoryStream(XPacketConverter.Serialize(
                        (byte)XPacketType.Handshake,
                        (byte)XPacketType.Unknown,
                        new XPacketHandshake
                        {
                            MagicHandshakeNumber = HandshakeMagic
                        }).ToPacket());

            
            var resp = await client.PostAsync("http://localhost:8888/",new StreamContent(outStream));

            var respBytes = await resp.Content.ReadAsByteArrayAsync();

            var xpacket = XPacket.Parse(respBytes);

            ProcessHandshake(xpacket);
        }

        static void ProcessIncomingPacket(XPacket packet)
        {
            var type = XPacketTypeManager.GetTypeFromPacket(packet);

            switch (type)
            {
                case XPacketType.Handshake:
                    ProcessHandshake(packet);
                    break;
                case XPacketType.Unknown:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        static void ProcessHandshake(XPacket packet)
        {
            var handshake = XPacketConverter.Deserialize<XPacketHandshake>(packet);

            if (HandshakeMagic - handshake.MagicHandshakeNumber == 15)
            {
                Console.WriteLine("Handshake successful!");
            }
        }
    }
}