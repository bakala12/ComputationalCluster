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
        private static ClusterListenerFactory _instance;
        private static readonly object SyncRoot = new object();

        private ClusterListenerFactory() { }

        public static IClusterListenerFactory Factory
        {
            get
            {
                lock (SyncRoot)
                {
                    if(_instance==null)
                        _instance=new ClusterListenerFactory();
                }
                return _instance;
            }
        }

        /// <summary>
        /// potential use in mocking only
        /// </summary>
        /// <param name="adapter"></param>
        /// <returns></returns>
        public IClusterListener Create(ITcpListener adapter)
        {
            return new ClusterListener(adapter);
        }

        /// <summary>
        /// this overload should be used in components' code
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public IClusterListener Create(IPAddress address, int port)
        {
            ITcpListener listener = TcpListenerAdapterFactory.Factory.Create(address, port);
            return new ClusterListener(listener);
        }
    }
}
