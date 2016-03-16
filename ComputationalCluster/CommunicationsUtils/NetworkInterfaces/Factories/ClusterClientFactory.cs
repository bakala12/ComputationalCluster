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
        IClusterClient Create(string hostname, int port, IClientAdapterFactory factory);
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
        public IClusterClient Create(string hostname, int port, 
            IClientAdapterFactory factory)
        {
            return new ClusterClient(hostname, port, factory);
        }

        /// <summary>
        /// this overload should be used in components' code
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public IClusterClient Create(string hostname, int port)
        {
            IClientAdapterFactory factory = new TcpClientAdapterFactory();
            return new ClusterClient(hostname, port, factory);
        }
    }
}
