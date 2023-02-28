using System.Linq;
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
            this._data = new(data);
        }
    }
}