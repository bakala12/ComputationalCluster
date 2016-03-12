using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommunicationsUtils.NetworkInterfaces.Adapters;
using CommunicationsUtils.NetworkInterfaces.Factories;
using CommunicationsUtils.NetworkInterfaces;
using CommunicationsUtils.Messages;

namespace Tests
{
    [TestClass]
    public class NetworkInterfacesTests
    {
        /// <summary>
        /// no assertion needed. this test will fail when clusterclient would fail
        /// </summary>
        [TestMethod]
        public void ClusterClientTest()
        {
            ITcpClient adapter = MockClientAdapterFactory.Factory.Create();
            ClusterClient client = new ClusterClient("0", 0, adapter);
            client.SendRequests(new Message[] {new DivideProblem () { ProblemType = "example"},
            new Status { Id = 123456789 }});
        }
    }
}
