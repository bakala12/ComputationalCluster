using CommunicationsUtils.ClientComponentCommon;
using CommunicationsUtils.NetworkInterfaces;
using CommunicationsUtils.NetworkInterfaces.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Core;

namespace TaskManager
{
    class Program
    {
        static void Main(string[] args)
        {
            //args parsing

            IClusterClient statusClient = ClusterClientFactory.Factory.Create(
                Properties.Settings.Default.Address, Properties.Settings.Default.Port);
            IClusterClient problemClient = ClusterClientFactory.Factory.Create(
                Properties.Settings.Default.Address, Properties.Settings.Default.Port);
            //factory will be here in the future:
            var newCore = TaskManagerProcessingModuleFactory.Factory.Create
                (new List<string> { "DVRP" });

            var creator = new MessageArrayCreator();

            TaskManager taskManager = new TaskManager(statusClient, problemClient, creator,
                newCore);

            taskManager.Run();
        }
    }
}
