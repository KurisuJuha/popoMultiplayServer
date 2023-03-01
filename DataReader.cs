using System;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace JuhaKurisu.PopoTools.ByteSerializer
{
    public class DataReader
    {
        public ReadOnlyCollection<byte> bytes => Array.AsReadOnly(byteArray);
        public int position { get; private set; }

        private readonly byte[] byteArray;

        public DataReader(byte[] bytes)
        {
            byteArray = bytes;
        }

        public DataReader(ReadOnlyCollection<byte> bytes)
        {
            this.byteArray = bytes.ToArray();
        }

        public bool ReadBoolean()
            => BitConverter.ToBoolean(new byte[] { ReadByte() }, 0);

        public Byte ReadByte()
            => bytes[++position - 1];

        public SByte ReadSByte()
            => (sbyte)ReadByte();

        public Int16 ReadShort()
            => BitConverter.ToInt16(ReadBytes(2), 0);

        public UInt16 ReadUShort()
            => BitConverter.ToUInt16(ReadBytes(2), 0);

        public Int32 ReadInt()
            => BitConverter.ToInt32(ReadBytes(4), 0);

        public UInt32 ReadUInt()
            => BitConverter.ToUInt32(ReadBytes(4), 0);

        public Int64 ReadLong()
            => BitConverter.ToInt64(ReadBytes(8), 0);

        public UInt64 ReadULong()
            => BitConverter.ToUInt64(ReadBytes(8), 0);

        public Char ReadChar()
            => BitConverter.ToChar(ReadBytes(2), 0);

        public Single ReadFloat()
            => BitConverter.ToSingle(ReadBytes(4), 0);

        public Double ReadDouble()
            => BitConverter.ToDouble(ReadBytes(8), 0);

        public Byte[] ReadBytes()
            => ReadBytes(ReadInt());

        public Byte[] ReadBytes(int length)
            => byteArray[position..(position += length)];

        public string ReadString()
            => Encoding.UTF8.GetString(ReadBytes());

        public DataWriter ReadDataWriter()
            => new DataWriter(ReadBytes());

        public DataReader ReadDataReader()
            => new DataReader(ReadBytes());

        public Guid ReadGuid()
            => new Guid(ReadBytes(16));
    }
}