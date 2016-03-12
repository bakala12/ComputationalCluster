using CommunicationsUtils.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace CommunicationsUtils.Serialization
{
    public interface IMessageSerializer
    {
        Message FromXmlString(string xml);
        string ToXmlString(Message message);
    }

    public class MessagesSerializer : IMessageSerializer
    {
        public Message FromXmlString(string xml)
        {
            StringReader stream = new StringReader(xml);
            XmlReader reader = XmlReader.Create(stream);
            var node = reader.MoveToContent();
            string s = reader.LocalName;
            var messageType = typeof (Message).Assembly.GetType("CommunicationsUtils.Messages." + s);
            var type = typeof(XmlStringSerializer<>).MakeGenericType(messageType);
            var serializer = Activator.CreateInstance(type);
            var method = type.GetMethod("FromXmlString");
            return (Message)method.Invoke(serializer, new object[] { xml });
        }

        public string ToXmlString(Message message)
        {
            var serializer = typeof(XmlStringSerializer<>).MakeGenericType(message.GetType());
            var method = serializer.GetMethod("ToXmlString");
            var ser = Activator.CreateInstance(serializer);
            return (string)method.Invoke(ser, new object[] { message });
        }
    }
}
