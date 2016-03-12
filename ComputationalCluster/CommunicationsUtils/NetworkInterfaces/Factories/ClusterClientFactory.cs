using CommunicationsUtils.NetworkInterfaces.Adapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationsUtils.NetworkInterfaces.Factories
{
    public interface IClusterClientFactory
    {
        IClusterClient Create(string hostname, int port);
        IClusterClient Create(string hostname, int port, ITcpClient adapter);
    }

    public class ClusterClientFactory : IClusterClientFactory
    {
        private static ClusterClientFactory instance = new ClusterClientFactory();

        public static ClusterClientFactory Factory
        {
            get
            {
                return instance;
            }
        }

        public IClusterClient Create(string hostname, int port, ITcpClient adapter)
        {
            return new ClusterClient(hostname, port, adapter);
        }

        public IClusterClient Create(string hostname, int port)
        {
            ITcpClient adapter = TcpClientAdapterFactory.Factory.Create();
            return new ClusterClient(hostname, port, adapter);
        }
    }
}
