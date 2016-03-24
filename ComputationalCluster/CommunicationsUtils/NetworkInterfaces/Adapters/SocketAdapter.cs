using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

        public int Receive(byte[] requestBytes, int count)
        {
            return wrappedSocket.Receive(requestBytes, 0, count, SocketFlags.None);
        }

        public void Send(byte[] v, int count)
        {
            wrappedSocket.Send(v, count, SocketFlags.None);
        }

        public string ExtractSocketAddress()
        {
            IPEndPoint endPoint = (IPEndPoint)wrappedSocket.RemoteEndPoint;
            return endPoint.Address.ToString();
        }

        public int ExtractSocketPort()
        {
            IPEndPoint endPoint = (IPEndPoint)wrappedSocket.RemoteEndPoint;
            return endPoint.Port;
        }
    }
}
