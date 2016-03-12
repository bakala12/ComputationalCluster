using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationsUtils.NetworkInterfaces.Adapters
{
    public class MockSocketAdapter : ISocket
    {
        public void Close()
        {
            return;
        }

        public void Receive(byte[] requestBytes)
        {
            return;
        }

        public void Send(byte[] v)
        {
            return;
        }
    }
}
