using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationsUtils.NetworkInterfaces
{
    public class MockNetworkStream : INetworkStream
    {
        public void Read(byte[] buf, int offset, int length)
        {
            return;
        }

        public void Write(byte[] buf, int offset, int length)
        {
            return;
        }
    }
}
