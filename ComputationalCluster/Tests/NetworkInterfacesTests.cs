using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommunicationsUtils.NetworkInterfaces;
using CommunicationsUtils.Messages;

namespace Tests
{
    [TestClass]
    public class NetworkInterfacesTests
    {
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
