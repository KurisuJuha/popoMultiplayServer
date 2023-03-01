using System.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace JuhaKurisu.PopoTools.Multiplay
{
    public class Message
    {
        public readonly MessageType type;
        public byte[] data => _data.ToArray();

        private readonly ReadOnlyCollection<byte> _data;

        public Message(MessageType type, byte[] data)
        {
            this.type = type;
            this._data = new(data);
        }

        public byte[] ToBytes()
        {
            List<byte> ret = new();

            ret.Add((byte)type);
            ret.AddRange(BitConverter.GetBytes(_data.Count));
            ret.AddRange(data);

            return ret.ToArray();
        }
    }
}