using System;
using System.Collections.Concurrent;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using CommunicationsUtils.Messages;
using CommunicationsUtils.NetworkInterfaces;
using CommunicationsUtils.NetworkInterfaces.Adapters;
using CommunicationsUtils.NetworkInterfaces.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Server.Data;
using Server.MessageProcessing;

namespace Tests
{
    /// <summary>
    /// Summary description for MessageProcessingTests
    /// </summary>
    [TestClass]
    public class MessageProcessingTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            var queue = new ConcurrentQueue<Message>();
            queue.Enqueue(new Status() { Id = 5 });
            var tab = queue.ToArray();
            queue = new ConcurrentQueue<Message>();
            Assert.AreNotEqual(tab[0], null);
        }

        [TestMethod]
        public void BackupRegisteringTest1()
        {
            var backup = new List<BackupServerInfo>();
            MessageProcessor processor = new PrimaryMessageProcessor
                (new ClusterListener( new MockListenerAdapter()), new ConcurrentQueue<Message>(), new ConcurrentDictionary<int, ProblemDataSet>(), new ConcurrentDictionary<int, ActiveComponent>() );
            var register = new Register()
            {
                Type = new RegisterType()
                {
                    Value = ComponentType.CommunicationServer,
                    port = 8086,
                    portSpecified = true
                },
            };
            Message[] response = processor.CreateResponseMessages(register, new Dictionary<int, ProblemDataSet>(),
                new Dictionary<int, ActiveComponent>(), backup);
            NoOperation nop = response.OfType<NoOperation>().FirstOrDefault();
            Assert.AreEqual(backup.Count, nop.BackupServersInfo.Length);
        }

        [TestMethod]
        public void ComponentRegisteringTest1()
        {
            var backup = new List<BackupServerInfo>();
            MessageProcessor processor = new PrimaryMessageProcessor
                (new ClusterListener(new MockListenerAdapter()), new ConcurrentQueue<Message>(), new ConcurrentDictionary<int, ProblemDataSet>(), new ConcurrentDictionary<int, ActiveComponent>() );
            var register = new Register()
            {
                Type = new RegisterType()
                {
                    Value = ComponentType.ComputationalNode,
                },
                ParallelThreads = 1
            };
            Message[] response = processor.CreateResponseMessages(register, new Dictionary<int, ProblemDataSet>(),
                new Dictionary<int, ActiveComponent>(), backup);
            NoOperation nop = response.OfType<NoOperation>().FirstOrDefault();
            Assert.AreEqual(0,backup.Count);
            Assert.AreEqual(0, nop.BackupServersInfo.Length);
        }


    }
}
