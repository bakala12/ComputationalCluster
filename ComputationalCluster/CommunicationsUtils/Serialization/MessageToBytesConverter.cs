using CommunicationsUtils.Messages;
using CommunicationsUtils.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationsUtils.Serialization
{
    public class MessageToBytesConverter
    {
        private readonly MessagesSerializer _serializer = new MessagesSerializer();

        public byte[] ToByteArray(Message message)
        {
            return Encoding.UTF8.GetBytes(_serializer.ToXmlString(message));
        }

        public Message FromBytesArray(byte[] bytes)
        {
            return _serializer.FromXmlString(Encoding.UTF8.GetString(bytes));
        }

        /// <summary>
        /// Divides byte array to multiple messages
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public Message[] BytesToMessages(byte[] bytes)
        {
            List<Message> messages = new List<Message>();
            int msgStart = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                if (bytes[i] == 23)
                {
                    byte[] chunk = bytes.GetSubarray(msgStart, i - 1);
                    messages.Add(this.FromBytesArray(chunk));
                    msgStart = i + 1;
                }
            }
            return messages.ToArray();
        }

        public byte[][] MessagesToBytes (Message[] messages)
        {
            byte[][] bytesChunks = new byte[messages.Length][];
            for (int i = 0; i < messages.Length; i++)
            {
                byte[] requestBytes = this.ToByteArray(messages[i]);
                bytesChunks[i] = requestBytes;
            }
            return bytesChunks;
        }
    }
}
