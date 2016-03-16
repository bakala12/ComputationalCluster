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
        void Write(byte[] buf, int count);
        int Read(byte[] buf, int count);
        void Close();
    }

    public interface ITcpListener
    {
        void Start();
        ISocket AcceptSocket();
        void Stop();
    }

    public interface ISocket
    {
        int Receive(byte[] requestBytes, int count);
        void Close();
        void Send(byte[] v, int count);
    }
}
