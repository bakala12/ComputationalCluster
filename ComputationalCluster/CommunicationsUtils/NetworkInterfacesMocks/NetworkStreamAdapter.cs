using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationsUtils.NetworkInterfaces
{
    public class NetworkStreamAdapter : INetworkStream
    {
        private Stream wrappedStream;

        public NetworkStreamAdapter(Stream _stream)
        {
            wrappedStream = _stream;
        }

        public void Read(byte[] buf, int offset, int count)
        {
            wrappedStream.Read(buf, offset, count);
        }

        public void Write(byte[] buf, int offset, int count)
        {
            wrappedStream.Write (buf, offset, count);
        }
    }

    public interface INetworkStream
    {
        void Write(byte[] buf, int offset, int length);
        void Read(byte[] buf, int offset, int length);
    }
}
