using System;
using System.Collections.Concurrent;
using System.Text;
using System.Collections.Generic;
using CommunicationsUtils.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
    }
}
