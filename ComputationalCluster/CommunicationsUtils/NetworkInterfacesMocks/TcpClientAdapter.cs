using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationsUtils.NetworkInterfaces
{
    public class TcpClientAdapter : ITcpClient
    {
        private TcpClient wrappedClient;
        public TcpClientAdapter(TcpClient client)
        {
            wrappedClient = client;
        }

        public void Close()
        {
            wrappedClient.Close();
        }

        public void Connect(string hostname, int port)
        {
            wrappedClient.Connect(hostname, port);
        }

        public INetworkStream GetStream()
        {
            return new NetworkStreamAdapter(wrappedClient.GetStream());
        }
    }

    public interface ITcpClient
    {
        INetworkStream GetStream();
        void Connect(string hostname, int port);
        void Close();
    }
}
