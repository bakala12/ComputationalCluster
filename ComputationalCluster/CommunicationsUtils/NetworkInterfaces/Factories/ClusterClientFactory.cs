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

        /// <summary>
        /// potential use in mocking only
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="port"></param>
        /// <param name="adapter"></param>
        /// <returns></returns>
        public IClusterClient Create(string hostname, int port, ITcpClient adapter)
        {
            return new ClusterClient(hostname, port, adapter);
        }

        /// <summary>
        /// this overload should be used in components' code
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public IClusterClient Create(string hostname, int port)
        {
            ITcpClient adapter = TcpClientAdapterFactory.Factory.Create();
            return new ClusterClient(hostname, port, adapter);
        }
    }
}
