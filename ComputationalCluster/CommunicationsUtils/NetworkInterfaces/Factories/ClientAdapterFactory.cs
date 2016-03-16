using System.Net.Sockets;
using CommunicationsUtils.NetworkInterfaces.Adapters;
using CommunicationsUtils.NetworkInterfaces.Mocks;

namespace CommunicationsUtils.NetworkInterfaces.Factories
{
    /// <summary>
    /// factory creating mocked and real client for communication
    /// </summary>

    public interface IClientAdapterFactory
    {
        ITcpClient Create();
    }

    public class TcpClientAdapterFactory : IClientAdapterFactory
    {

        public ITcpClient Create()
        {
            return new TcpClientAdapter( new TcpClient() );
        }
    }

    public class MockClientAdapterFactory : IClientAdapterFactory
    {

        public ITcpClient Create()
        {
            return new MockClientAdapter();
        }
    }
}
