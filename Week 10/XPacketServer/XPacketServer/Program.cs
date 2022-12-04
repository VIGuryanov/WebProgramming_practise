using System;
using System.Net;
using System.Text;
using MyProtocol;

namespace XPacketServer
{
    internal class Program
    {
        static void Main(string[] args)
        {



            var listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:8888/");
            listener.Start();
            var context = listener.GetContext();
            using var strReader = new BinaryReader(context.Request.InputStream);
            var content = XPacket.Parse(strReader.ReadBytes((int)context.Request.ContentLength64));
            ProcessHandshake(content, context.Response);

        }

        static void ProcessHandshake(XPacket packet, HttpListenerResponse resp)
        {
            Console.WriteLine("Recieved handshake packet.");

            var handshake = XPacketConverter.Deserialize<XPacketHandshake>(packet);
            handshake.MagicHandshakeNumber -= 15;

            Console.WriteLine("Answering..");

            var outVal = XPacketConverter.Serialize((byte)XPacketType.Handshake, (byte)XPacketType.Unknown, handshake)
                    .ToPacket();

            var respStream = resp.OutputStream;
            resp.ContentLength64 = outVal.Length;

            respStream.Write(outVal, 0, outVal.Length);
            /*QueuePacketSend(
                XPacketConverter.Serialize(XPacketType.Handshake, handshake)
                    .ToPacket());*/
        }
    }
}