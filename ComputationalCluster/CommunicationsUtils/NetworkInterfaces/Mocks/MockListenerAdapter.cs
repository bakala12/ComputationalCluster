using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationsUtils.NetworkInterfaces.Mocks
{
    public class MockListenerAdapter : ITcpListener
    {
        public ISocket AcceptSocket()
        {
            return new MockSocketAdapter();
        }

        public void Start()
        {
            return;
        }

        public void Stop()
        {
            return;
        }
    }
}
