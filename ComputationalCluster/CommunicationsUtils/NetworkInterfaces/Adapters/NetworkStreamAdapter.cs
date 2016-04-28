using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationsUtils.NetworkInterfaces.Adapters
{
    public class NetworkStreamAdapter : INetworkStream
    {
        private Stream wrappedStream;

        public NetworkStreamAdapter(Stream _stream)
        {
            wrappedStream = _stream;
        }

        public void Close()
        {
            wrappedStream.Close();
        }

        //TO DO
        public void Dispose()
        {
            return;
        }

        public int Read(byte[] buf, int count)
        {
            int len = wrappedStream.Read(buf, 0, count);
            wrappedStream.Flush();
            return len;
        }

        public void Write(byte[] buf, int count)
        {
            wrappedStream.Write (buf, 0, count);
            wrappedStream.Flush();
        }
    }
}
