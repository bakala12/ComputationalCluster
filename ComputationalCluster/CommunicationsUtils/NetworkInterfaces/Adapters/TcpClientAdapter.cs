using System.Net.Sockets;

namespace CommunicationsUtils.NetworkInterfaces.Adapters
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
}
