using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationsUtils.NetworkInterfaces.Adapters
{
    public class MockClientAdapter : ITcpClient
    {
        public void Close()
        {
            return;
        }

        public void Connect(string hostname, int port)
        {
            return;
        }

        public INetworkStream GetStream()
        {
            return new MockNetworkStream();
        }
    }
}
