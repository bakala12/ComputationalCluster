using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationsUtils.NetworkInterfaces.Mocks
{
    public class MockSocketAdapter : ISocket
    {
        public void Close()
        {
            return;
        }

        public int Receive(byte[] requestBytes, int count)
        {
            return 0;
        }

        public void Send(byte[] v, int count)
        {
            return;
        }
    }
}
