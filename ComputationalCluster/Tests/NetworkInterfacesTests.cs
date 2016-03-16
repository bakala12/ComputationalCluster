using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommunicationsUtils.NetworkInterfaces.Adapters;
using CommunicationsUtils.NetworkInterfaces.Factories;
using CommunicationsUtils.NetworkInterfaces;
using CommunicationsUtils.Messages;
using System.Net;

namespace Tests
{
    [TestClass]
    public class NetworkInterfacesTests
    {
        /// <summary>
        /// no assertion needed. this test will fail when clusterclient methods would fail
        /// </summary>
        [TestMethod]
        public void BasicClusterClientTest()
        {
            IClientAdapterFactory factory = new MockClientAdapterFactory();
            IClusterClient client = ClusterClientFactory.Factory.Create("0", 0, factory);
            client.SendRequests(new Message[] {new DivideProblem () { ProblemType = "example"},
            new Status { Id = 123456789 }});
        }

        /// <summary>
        /// same as above
        /// </summary>
        [TestMethod]
        public void BasicClusterListenerTest()
        {
            ITcpListener adapter = MockListenerAdapterFactory.Factory.Create(IPAddress.Any, 0);
            IClusterListener listener = ClusterListenerFactory.Factory.Create(adapter);
            listener.Start();
            Message[] requests = listener.WaitForRequest();
            listener.SendResponse(new Message[] {new DivideProblem () { ProblemType = "example"},
            new Status { Id = 123456789 }});
            listener.Stop();
        }
    }
}
