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
        private static TcpClientAdapterFactory _instance;
        private static readonly object SyncRoot = new object();

        private TcpClientAdapterFactory() { }

        public static IClientAdapterFactory Factory
        {
            get
            {
                lock (SyncRoot)
                {
                    if(_instance==null)
                        _instance = new TcpClientAdapterFactory();
                }
                return _instance;
            }
        }

        public ITcpClient Create()
        {
            return new TcpClientAdapter( new TcpClient() );
        }
    }

    public class MockClientAdapterFactory : IClientAdapterFactory
    {
        private static MockClientAdapterFactory _instance;
        private static readonly object SyncRoot = new object();

        private MockClientAdapterFactory() { }

        public static IClientAdapterFactory Factory
        {
            get
            {
                lock (SyncRoot)
                {
                    if(_instance==null)
                        _instance = new MockClientAdapterFactory();
                }
                return _instance;
            }
        }

        public ITcpClient Create()
        {
            return new MockClientAdapter();
        }
    }
}
