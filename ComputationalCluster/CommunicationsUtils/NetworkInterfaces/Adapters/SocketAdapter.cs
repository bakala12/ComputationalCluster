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
            int len = 0;
            using (var stream = new NetworkStream(wrappedSocket))
            {
                len = stream.Read(requestBytes, 0, count);
            }
            return len;
        }

        public void Send(byte[] v, int count)
        {
            using (var stream = new NetworkStream(wrappedSocket))
            {
                stream.Write(v, 0, count);
            }
        }

        public string ExtractSocketAddress()
        {
            IPEndPoint endPoint = (IPEndPoint)wrappedSocket.RemoteEndPoint;
            return endPoint.Address.ToString();
        }

        public void KillSocket()
        {
            return;
        }
    }
}
