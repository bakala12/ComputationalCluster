using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
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

    public interface ITcpListener
    {
        void Start();
        ISocket AcceptSocket();
        void Stop();
    }

    public interface ISocket
    {
        void Receive(byte[] requestBytes);
        void Close();
        void Send(byte[] v);
    }
}
