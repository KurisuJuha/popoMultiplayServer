using System.Linq;
using JuhaKurisu.PopoTools.ByteSerializer;

namespace JuhaKurisu.PopoTools.Multiplay.Extentions
{
    public static class MessageSerializerEx
    {
        public static DataWriter Append(this DataWriter writer, Message message)
        {
            writer.Append((byte)message.type);
            writer.AppendWithLength(message.data.ToArray());
            return writer;
        }

        public static Message ReadMessage(this DataReader reader)
        {
            MessageType type = (MessageType)reader.ReadByte();
            byte[] data = reader.ReadBytes();
            return new Message(type, data);
        }
    }
}