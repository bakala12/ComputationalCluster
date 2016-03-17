using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicationsUtils.ClientComponentCommon;
using CommunicationsUtils.NetworkInterfaces;
using CommunicationsUtils.NetworkInterfaces.Factories;
using ComputationalNode.Core;

namespace ComputationalNode
{
    class Program
    {
        static void Main(string[] args)
        {
            // args parsing

            IClusterClient statusClient = ClusterClientFactory.Factory.Create(
                Properties.Settings.Default.Address, Properties.Settings.Default.Port);
            IClusterClient problemClient = ClusterClientFactory.Factory.Create(
                Properties.Settings.Default.Address, Properties.Settings.Default.Port);

            var newCore = ComputationalNodeProcessingModuleFactory.Factory.Create();

            var creator = new MessageArrayCreator();

            ComputationalNode computationalNode = new ComputationalNode(statusClient, problemClient, creator, newCore);

            computationalNode.Run();
        }
    }
}
