using System.Net.Sockets;
using CommunicationsUtils.NetworkInterfaces.Adapters;
using System.Net;
using CommunicationsUtils.NetworkInterfaces.Mocks;

namespace CommunicationsUtils.NetworkInterfaces.Factories
{
    /// <summary>
    /// factory creating mocked and real client for communication
    /// </summary>

    public interface IListenerAdapterFactory
    {
        ITcpListener Create(IPAddress address, int port);
    }

    public class TcpListenerAdapterFactory : IListenerAdapterFactory
    {
        private static TcpListenerAdapterFactory instance = new TcpListenerAdapterFactory();

        public static TcpListenerAdapterFactory Factory
        {
            get
            {
                return instance;
            }
        }

        public ITcpListener Create(IPAddress hostname, int port)
        {
            return new TcpListenerAdapter(new TcpListener(hostname, port));
        }
    }

    public class MockListenerAdapterFactory : IListenerAdapterFactory
    {
        private static MockListenerAdapterFactory instance = new MockListenerAdapterFactory();

        public static MockListenerAdapterFactory Factory
        {
            get
            {
                return instance;
            }
        }

        public ITcpListener Create(IPAddress address, int port)
        {
            return new MockListenerAdapter();
        }
    }
}
