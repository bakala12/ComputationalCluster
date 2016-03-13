using CommunicationsUtils.Messages;
using CommunicationsUtils.NetworkInterfaces;
using CommunicationsUtils.NetworkInterfaces.Adapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationsUtils.ClientComponentCommon
{
    //common things for client components (that means TM and CN, but NO CC)
    public abstract class ClientComponent
    {
        protected IClusterClient clusterClient;
        protected ulong componentId;
        protected uint timeout;

        public ClientComponent(IClusterClient _clusterClient)
        {
            clusterClient = _clusterClient;
        }

        protected abstract void registerComponent();
    }
}
