using CommunicationsUtils.NetworkInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager
{
    public class TaskManager
    {
        IClusterClient clusterClient;

        public TaskManager(IClusterClient _clusterClient)
        {
            clusterClient = _clusterClient;
        }

        public void Run ()
        {

        }
    }
}
