using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicationsUtils.Serialization;
using CommunicationsUtils.Messages;

namespace Tests
{
    [TestClass]
    public class MessageSerializationTests
    {
        private MessagesSerializer _serializer = new MessagesSerializer();

        [TestMethod]
        public void MessageSerializationTest1()
        {
            Message message = MessagesFactory.CreateEmptyMessage(MessageType.StatusMessage);
            Status statusMessage = message.Cast<Status>();
            statusMessage.Threads = new StatusThread[] { };
            statusMessage.Id = 123;
            string xml = _serializer.ToXmlString(statusMessage);
            Message m = _serializer.FromXmlString(xml);
            Status statusMessageDeserialized = m.Cast<Status>();
            Assert.AreEqual(statusMessage.Id, statusMessageDeserialized.Id);
            Assert.AreEqual(statusMessage.Threads?.Length, statusMessageDeserialized.Threads?.Length);
        }
    }
}
