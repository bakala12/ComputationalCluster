using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationsUtils.NetworkInterfaces.Adapters
{
    public class SocketAdapter : ISocket
    {
        private Socket wrappedSocket;

        public SocketAdapter(Socket _socket)
        {
            wrappedSocket = _socket;
        }

        public void Close()
        {
            wrappedSocket.Close();
        }

        public void Receive(byte[] requestBytes)
        {
            wrappedSocket.Receive(requestBytes);
        }

        public void Send(byte[] v)
        {
            wrappedSocket.Send(v);
        }
    }
}
