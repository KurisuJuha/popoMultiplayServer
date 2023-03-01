using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Linq;

namespace JuhaKurisu.PopoTools.ByteSerializer
{
    public class DataWriter
    {
        public ReadOnlyCollection<byte> bytes => byteList.AsReadOnly();
        private List<byte> byteList = new List<byte>();

        public DataWriter() { }

        public DataWriter(byte[] bytes)
        {
            byteList.AddRange(bytes);
        }

        public DataWriter Append(Boolean value)
        {
            byteList.AddRange(BitConverter.GetBytes(value));
            return this;
        }

        public DataWriter Append(Byte value)
        {
            byteList.Add(value);
            return this;
        }

        public DataWriter Append(Int16 value)
        {
            byteList.AddRange(BitConverter.GetBytes(value));
            return this;
        }

        public DataWriter Append(UInt16 value)
        {
            byteList.AddRange(BitConverter.GetBytes(value));
            return this;
        }

        public DataWriter Append(Int32 value)
        {
            byteList.AddRange(BitConverter.GetBytes(value));
            return this;
        }

        public DataWriter Append(UInt32 value)
        {
            byteList.AddRange(BitConverter.GetBytes(value));
            return this;
        }

        public DataWriter Append(Int64 value)
        {
            byteList.AddRange(BitConverter.GetBytes(value));
            return this;
        }

        public DataWriter Append(UInt64 value)
        {
            byteList.AddRange(BitConverter.GetBytes(value));
            return this;
        }

        public DataWriter Append(Char value)
        {
            byteList.AddRange(BitConverter.GetBytes(value));
            return this;
        }

        public DataWriter Append(Single value)
        {
            byteList.AddRange(BitConverter.GetBytes(value));
            return this;
        }

        public DataWriter Append(Double value)
        {
            byteList.AddRange(BitConverter.GetBytes(value));
            return this;
        }

        public DataWriter Append(byte[] bytes)
        {
            foreach (var b in bytes) Append(b);

            return this;
        }

        public DataWriter AppendWithLength(byte[] bytes)
        {
            Append(bytes.Length);
            Append(bytes);
            return this;
        }

        public DataWriter Append(string value)
        {
            AppendWithLength(Encoding.UTF8.GetBytes(value));
            return this;
        }

        public DataWriter Append(DataWriter writer)
        {
            AppendWithLength(writer.bytes.ToArray());
            return this;
        }

        public DataWriter Append(DataReader reader)
        {
            AppendWithLength(reader.bytes.ToArray());
            return this;
        }

        public DataWriter Append(Guid value)
        {
            Append(value.ToByteArray());
            return this;
        }
    }
}