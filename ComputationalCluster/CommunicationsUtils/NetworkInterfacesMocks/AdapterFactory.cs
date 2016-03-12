using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationsUtils.NetworkInterfaces
{
    public interface IAdapterFactory
    {
        ITcpClient Create();
    }

    public class TcpClientAdapterFactory : IAdapterFactory
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

    public class MockClientAdapterFactory : IAdapterFactory
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
