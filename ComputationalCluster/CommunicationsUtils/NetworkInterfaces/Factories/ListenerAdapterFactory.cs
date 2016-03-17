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
        private static TcpListenerAdapterFactory _instance;
        private static readonly object SyncRoot = new object();

        private TcpListenerAdapterFactory() { }

        public static IListenerAdapterFactory Factory
        {
            get
            {
                lock (SyncRoot)
                {
                    if (_instance == null)
                    {
                        _instance = new TcpListenerAdapterFactory();
                    }
                }
                return _instance;
            }
        }

        public ITcpListener Create(IPAddress hostname, int port)
        {
            return new TcpListenerAdapter(new TcpListener(hostname, port));
        }
    }

    public class MockListenerAdapterFactory : IListenerAdapterFactory
    {
        private static MockListenerAdapterFactory _instance;
        private static readonly object SyncRoot = new object();

        private MockListenerAdapterFactory() { }

        public static IListenerAdapterFactory Factory
        {
            get
            {
                lock (SyncRoot)
                {
                    if (_instance == null)
                    {
                        _instance=new MockListenerAdapterFactory();
                    }
                }
                return _instance;
            }
        }

        public ITcpListener Create(IPAddress address, int port)
        {
            return new MockListenerAdapter();
        }
    }
}
