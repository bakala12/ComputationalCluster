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
        public Message[] BytesToMessages(byte[] bytes, int len)
        {
            List<Message> messages = new List<Message>();
            int msgStart = 0;
            for (int i = 0; i < len; i++)
            {
                if (bytes[i] == 23)
                {
                    byte[] chunk = bytes.GetSubarray(msgStart, i - 1);
                    messages.Add(this.FromBytesArray(chunk));
                    msgStart = i + 1;
                }
            }
            //compatibility issue - our bytes end with ETB, 
            //other groups could not
            if (msgStart < len)
            {
                byte[] chunk = bytes.GetSubarray(msgStart, len-1);
                messages.Add(this.FromBytesArray(chunk));
            }

            return messages.ToArray();
        }

        /// <summary>
        /// Writes messages to one dimensional buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="messages"></param>
        /// <returns>total count of buffer's relevant bytes</returns>
        public int MessagesToBytes (out byte[] buffer, Message[] messages)
        {
            byte[][] bytesChunks = new byte[messages.Length][];
            for (int i = 0; i < messages.Length; i++)
            {
                byte[] requestBytes = this.ToByteArray(messages[i]);
                bytesChunks[i] = requestBytes;
            }
            return copyToBuffer(out buffer, bytesChunks);
        }

        /// <summary>
        /// Writing/reading via TCP must be called once to not make any errors.
        /// That's why we need to copy bytes' chunks to one dim buffer
        /// </summary>
        /// <param name="buffer">out buffer</param>
        /// <param name="messagesBytes">bytes' chunks</param>
        /// <returns>total count of buffer's relevant bytes</returns>
        private static int copyToBuffer (out byte[] buffer, byte[][] messagesBytes)
        {
            int count = 0;
            buffer = new byte[Properties.Settings.Default.MaxBufferSize];
            for (int i = 0; i < messagesBytes.GetLength(0); i++)
            {
                messagesBytes[i].CopyTo(buffer, count);
                count += messagesBytes[i].Length;
                buffer[count++] = (byte)23;
            }
            return count;
        }
    }
}
