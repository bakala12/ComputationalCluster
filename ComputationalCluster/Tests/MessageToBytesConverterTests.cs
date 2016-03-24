using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommunicationsUtils.Messages;
using CommunicationsUtils.Serialization;

namespace Tests
{
    [TestClass]
    public class MessageToBytesConverterTests
    {
        [TestMethod]
        public void OneMessageToBytesTest()
        {
            MessageToBytesConverter converter = new MessageToBytesConverter();
            Message msg = new NoOperation();
            byte[] bytes = converter.ToByteArray(msg);
            Message outMsg = converter.FromBytesArray(bytes);
            Assert.AreEqual(msg.GetType(), outMsg.GetType());
        }

        [TestMethod]
        public void MultipleMessagesToBytesTest()
        {
            MessageToBytesConverter converter = new MessageToBytesConverter();
            Message[] messages = new Message[] {
                new DivideProblem()
                {
                    ProblemType = "abc",
                    ComputationalNodes = 696
                }
                , new Register()
            {
                Type = new RegisterType()
                {
                    Value = ComponentType.CommunicationServer,
                },
                Id = 212,
                IdSpecified = true
            }, new Status ()
            {
                Id = 10,
                Threads = new StatusThread[] { new StatusThread () { ProblemType = "def" } }
            }
        };
            byte[] bytes;
            int count = converter.MessagesToBytes(out bytes, messages);
            Assert.IsTrue(count > 0);

            Message[] outMessages = converter.BytesToMessages(bytes, count);

            Assert.AreEqual(messages.Length, outMessages.Length);
            Assert.AreEqual(messages[0].GetType(), outMessages[0].GetType());
            Assert.AreEqual(messages[1].GetType(), outMessages[1].GetType());
            Assert.AreEqual(messages[2].GetType(), outMessages[2].GetType());
            Assert.AreEqual((messages[0] as DivideProblem).ComputationalNodes,
                (outMessages[0] as DivideProblem).ComputationalNodes);
            Assert.AreEqual((messages[0] as DivideProblem).ProblemType,
                (outMessages[0] as DivideProblem).ProblemType);
            Assert.AreEqual((messages[1] as Register).Type.Value, (outMessages[1] as Register).Type.Value);
            Assert.AreEqual((messages[1] as Register).Id, (outMessages[1] as Register).Id);
            Assert.AreEqual((messages[2] as Status).Threads.Length, (outMessages[2] as Status).Threads.Length);
            Assert.AreEqual((messages[2] as Status).Threads[0].ProblemType, 
                (outMessages[2] as Status).Threads[0].ProblemType);
            Assert.AreEqual((messages[2] as Status).Id, (outMessages[2] as Status).Id);

        }
    }
}
