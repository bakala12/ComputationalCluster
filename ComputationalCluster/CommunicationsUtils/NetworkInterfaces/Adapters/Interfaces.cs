using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationsUtils.NetworkInterfaces.Adapters
{ 
    public interface ITcpClient
    {
        INetworkStream GetStream();
        void Connect(string hostname, int port);
        void Close();
    }

    public interface INetworkStream : IDisposable
    {
        void Write(byte[] buf, int offset, int length);
        void Read(byte[] buf, int offset, int length);
    }
}
