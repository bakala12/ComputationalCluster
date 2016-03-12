using System.Net.Sockets;
using CommunicationsUtils.NetworkInterfaces.Adapters;

namespace CommunicationsUtils.NetworkInterfaces.Factories
{
    public interface IClientAdapterFactory
    {
        ITcpClient Create();
    }

    public class TcpClientAdapterFactory : IClientAdapterFactory
    {
        private static TcpClientAdapterFactory instance = new TcpClientAdapterFactory();

        public static TcpClientAdapterFactory Factory
        {
            get
            {
                return instance;
            }
        }

        public ITcpClient Create()
        {
            return new TcpClientAdapter( new TcpClient() );
        }
    }

    public class MockClientAdapterFactory : IClientAdapterFactory
    {
        private static MockClientAdapterFactory instance = new MockClientAdapterFactory();

        public static MockClientAdapterFactory Factory
        {
            get
            {
                return instance;
            }
        }

        public ITcpClient Create()
        {
            return new MockClientAdapter();
        }
    }
}
