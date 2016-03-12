using CommunicationsUtils.NetworkInterfaces.Adapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationsUtils.NetworkInterfaces.Factories
{
    public interface IClusterListenerFactory
    {
        IClusterListener Create(IPAddress address, int port);
        IClusterListener Create(ITcpListener adapter);
    }

    public class ClusterListenerFactory : IClusterListenerFactory
    {
        private static ClusterListenerFactory instance = new ClusterListenerFactory();

        public static ClusterListenerFactory Factory
        {
            get
            {
                return instance;
            }
        }

        public IClusterListener Create(ITcpListener adapter)
        {
            return new ClusterListener(adapter);
        }

        public IClusterListener Create(IPAddress address, int port)
        {
            ITcpListener listener = TcpListenerAdapterFactory.Factory.Create(address, port);
            return new ClusterListener(listener);
        }
    }
}
