using Client.Core;
using CommunicationsUtils.ClientComponentCommon;
using CommunicationsUtils.Miscellaneous;
using CommunicationsUtils.NetworkInterfaces;
using CommunicationsUtils.NetworkInterfaces.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {       
            //args parsing

            IClusterClient clusterClient = ClusterClientFactory.Factory.Create(
                Properties.Settings.Default.Address, Properties.Settings.Default.Port);
            var core = ClientNodeProcessingModuleFactory.Factory.Create();
            IMessageArrayCreator creator = new MessageArrayCreator();

            ClientNode clientNode = new ClientNode(clusterClient, core, creator);
            clientNode.Run();
        }
    }
}
