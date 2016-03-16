using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationsUtils.NetworkInterfaces.Adapters
{
    public class MockNetworkStream : INetworkStream
    {
        public void Close()
        {
            return;
        }

        public void Dispose()
        {
            return;
        }

        public int Read(byte[] buf, int count)
        {
            return 0;
        }

        public void Write(byte[] buf, int count)
        {
            return;
        }
    }
}
