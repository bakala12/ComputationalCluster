using System.Collections.Generic;
using CommunicationsUtils.Argument_parser;
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
            var parser = new ArgumentParser(OptionSetPool.ClientOptionsSet);
            parser.ProcessArguments(args);
            parser.UpdateConfiguration(parser.map);

            IClusterClient statusClient = ClusterClientFactory.Factory.Create(
                Properties.Settings.Default.Address, Properties.Settings.Default.Port);
            IClusterClient problemClient = ClusterClientFactory.Factory.Create(
                Properties.Settings.Default.Address, Properties.Settings.Default.Port);

            var newCore = ComputationalNodeProcessingModuleFactory.Factory.Create(new List<string> { "DVRP" });

            var creator = new MessageArrayCreator();

            ComputationalNode computationalNode = new ComputationalNode(statusClient, problemClient, creator, newCore);

            computationalNode.Run();
        }
    }
}
