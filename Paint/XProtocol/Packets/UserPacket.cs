using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XProtocol.Serializator;

namespace XProtocol.Packets
{
    public class UserPacket
    {
        [XField(0)]
        public NickNamePacket NickName;

        [XField(1)]
        public int Color;
    }

    public struct NickNamePacket
    {
        long part1;
        long part2;
        long part3;

        private NickNamePacket(long p1, long p2, long p3)
        {
            part1 = p1;
            part2 = p2;
            part3 = p3;
        }

        public static NickNamePacket EncodeString(string message)
        {
            if (message.Length > 18)
                throw new ArgumentException();

            var packet = new NickNamePacket(1, 1, 1);

            for (int i = 0; i < message.Length; i++)
            {
                if (i < 6)
                {
                    packet.part1 *= 1000;
                    packet.part1 += message[i];
                }
                else if (i < 13)
                {
                    packet.part2 *= 1000;
                    packet.part2 += message[i];
                }
                else
                {
                    packet.part3 *= 1000;
                    packet.part3 += message[i];
                }
            }

            return packet;
        }

        public string DecodeToString()
        {
            var sBuilder = new StringBuilder();
            var clone = new NickNamePacket(part1, part2, part3);
            while (true)
            {
                if (clone.part3 / 10 != 0)
                {
                    sBuilder.Append((char)(clone.part3 % 1000));
                    clone.part3 /= 1000;
                    continue;
                }
                if (clone.part2 / 10 != 0)
                {
                    sBuilder.Append((char)(clone.part2 % 1000));
                    clone.part2 /= 1000;
                    continue;
                }
                if (clone.part1 / 10 != 0)
                {
                    sBuilder.Append((char)(clone.part1 % 1000));
                    clone.part1 /= 1000;
                    continue;
                }
                break;
            }
            return new string(sBuilder.ToString().Reverse().ToArray());
        }
    }
}
