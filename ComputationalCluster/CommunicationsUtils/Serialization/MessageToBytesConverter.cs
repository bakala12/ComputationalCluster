using CommunicationsUtils.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationsUtils.Serialization
{
    public class MessageToBytesConverter
    {
        private MessagesSerializer _serializer = new MessagesSerializer();

        public byte[] ToByteArray(Message message)
        {
            return Encoding.UTF8.GetBytes(_serializer.ToXmlString(message));
        }

        public Message FromBytesArray(byte[] bytes)
        {
            return _serializer.FromXmlString(Encoding.UTF8.GetString(bytes));
        }
    }
}
