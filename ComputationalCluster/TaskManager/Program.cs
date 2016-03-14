using CommunicationsUtils.NetworkInterfaces;
using CommunicationsUtils.NetworkInterfaces.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager
{
    class Program
    {
        static void Main(string[] args)
        {
            //args parsing

            IClusterClient clusterClient = ClusterClientFactory.Factory.Create(
                Properties.Settings.Default.Address, Properties.Settings.Default.Port);
            //factory will be here in the future:
            var newCore = new TaskManagerProcessingModule();
            TaskManager taskManager = new TaskManager(clusterClient, newCore);

            taskManager.Run();
        }
    }
}
