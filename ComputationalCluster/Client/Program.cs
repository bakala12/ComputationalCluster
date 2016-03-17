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
using CommunicationsUtils.Argument_parser;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new ArgumentParser(OptionSetPool.ClientOptionsSet);
            parser.ProcessArguments(args);
            parser.UpdateConfiguration(parser.map);

            IClusterClient clusterClient = ClusterClientFactory.Factory.Create(
                Properties.Settings.Default.Address, Properties.Settings.Default.Port);
            var core = ClientNodeProcessingModuleFactory.Factory.Create();
            IMessageArrayCreator creator = new MessageArrayCreator();

            ClientNode clientNode = new ClientNode(clusterClient, core, creator);
            clientNode.Run();
        }
    }
}
