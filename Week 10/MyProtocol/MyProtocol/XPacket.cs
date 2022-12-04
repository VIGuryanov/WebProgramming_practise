using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MyProtocol
{
    public class XPacket
    {
        public byte PacketType { get; private set; }
        public byte PacketSubtype { get; private set; }
        public List<XPacketField> Fields { get; set; } = new List<XPacketField>();
        public bool Protected { get; set; }
        private bool ChangeHeaders { get; set; }

        private XPacket() { }

        public static XPacket Create(byte type, byte subtype)
        {
            return new XPacket
            {
                PacketType = type,
                PacketSubtype = subtype
            };
        }

        public XPacketField GetField(byte id)
        {
            foreach (var field in Fields)
            {
                if (field.FieldID == id)
                {
                    return field;
                }
            }

            return null;
        }

        public bool HasField(byte id)
        {
            return GetField(id) != null;
        }

        public T GetValue<T>(byte id) where T : struct
        {
            var field = GetField(id);

            if (field == null)
            {
                throw new Exception($"Field with ID {id} wasn't found.");
            }
            var neededSize = Marshal.SizeOf(typeof(T));

            if (field.FieldSize != neededSize)
            {
                throw new Exception($"Can't convert field to type {typeof(T).FullName}.\n" + $"We have {field.FieldSize} bytes but we need exactly {neededSize}.");
            }

            return Encoder.ByteArrayToFixedObject<T>(field.Contents);
        }

        public void SetValue(byte id, object structure)
        {
            if (!structure.GetType().IsValueType)
            {
                throw new Exception("Only value types are available.");
            }

            var field = GetField(id);

            if (field == null)
            {
                field = new XPacketField
                {
                    FieldID = id
                };

                Fields.Add(field);
            }

            var bytes = Encoder.FixedObjectToByteArray(structure);

            if (bytes.Length > byte.MaxValue)
            {
                throw new Exception("Object is too big. Max length is 255 bytes.");
            }

            field.FieldSize = (byte)bytes.Length;
            field.Contents = bytes;
        }

        public byte[] ToPacket()
        {
            var packet = new MemoryStream();

            packet.Write(
            new byte[] { 0xAF, 0xAA, 0xAF, PacketType, PacketSubtype }, 0, 5);

            var fields = Fields.OrderBy(field => field.FieldID);

            foreach (var field in fields)
            {
                packet.Write(new[] { field.FieldID, field.FieldSize }, 0, 2);
                packet.Write(field.Contents, 0, field.Contents.Length);
            }

            packet.Write(new byte[] { 0xFF, 0x00 }, 0, 2);

            return packet.ToArray();
        }

        public static XPacket Parse(byte[] packet)
        {
            if (packet.Length < 7)
            {
                return null;
            }

            if (packet[0] != 0xAF ||
                packet[1] != 0xAA ||
                packet[2] != 0xAF)
            {
                return null;
            }

            var mIndex = packet.Length - 1;

            if (packet[mIndex - 1] != 0xFF ||
                packet[mIndex] != 0x00)
            {
                return null;
            }

            var type = packet[3];
            var subtype = packet[4];

            var xpacket = Create(type, subtype);
            var fields = packet.Skip(5).ToArray();

            while (true)
            {
                if (fields.Length == 2)
                {
                    return xpacket;
                }

                var id = fields[0];
                var size = fields[1];

                var contents = size != 0 ?
                fields.Skip(2).Take(size).ToArray() : null;

                xpacket.Fields.Add(new XPacketField
                {
                    FieldID = id,
                    FieldSize = size,
                    Contents = contents
                });

                fields = fields.Skip(2 + size).ToArray();
            }
        }

        public void SetValueRaw(byte id, byte[] rawData)
        {
            var field = GetField(id);

            if (field == null)
            {
                field = new XPacketField
                {
                    FieldID = id
                };

                Fields.Add(field);
            }

            if (rawData.Length > byte.MaxValue)
            {
                throw new Exception("Object is too big. Max length is 255 bytes.");
            }

            field.FieldSize = (byte)rawData.Length;
            field.Contents = rawData;
        }

        public byte[] GetValueRaw(byte id)
        {
            var field = GetField(id);

            if (field == null)
            {
                throw new Exception($"Field with ID {id} wasn't found.");
            }

            return field.Contents;
        }
    }
    public class XPacketField
    {
        public byte FieldID { get; set; }
        public byte FieldSize { get; set; }
        public byte[] Contents { get; set; }
    }
}
