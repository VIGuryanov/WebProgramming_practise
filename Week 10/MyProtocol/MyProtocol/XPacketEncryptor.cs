using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using System.Text.Unicode;

namespace MyProtocol
{
    public class XProtocolEncryptor
    {
        private static string Key { get; } = "2e985f930853919313c96d001cb5701f";

        public static byte[] Encrypt(byte[] data)
        {
            //return Encoding.Unicode.GetBytes(RijndaelHandler.Encrypt(Encoding.Unicode.GetString(data), Key));
            return data;
        }

        public static byte[] Decrypt(byte[] data)
        {
            //return Encoding.Unicode.GetBytes(RijndaelHandler.Decrypt(Encoding.Unicode.GetString(data), Key));
            return data;
        }
    }
}
