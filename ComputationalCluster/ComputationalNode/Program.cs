using System.Collections.Generic;
using System.Linq;
using AlgorithmSolvers;
using CommunicationsUtils.Argument_parser;
using CommunicationsUtils.ClientComponentCommon;
using CommunicationsUtils.NetworkInterfaces;
using CommunicationsUtils.NetworkInterfaces.Factories;
using CommunicationsUtils.Shared;
using ComputationalNode.Core;
using log4net;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace ComputationalNode
{
    class Program
    {
        /// <summary>
        /// Even though we do not use logger in this class, there is a need to instantiate logger to set -verbose logging to console from starting parameters
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        static void Main(string[] args)
        {
            var parser = new ArgumentParser(OptionSetPool.ClientOptionsSet);
            parser.ProcessArguments(args);
            parser.UpdateConfiguration(parser.map);

            IClusterClient statusClient = ClusterClientFactory.Factory.Create(
                Properties.Settings.Default.Address, Properties.Settings.Default.Port);
            IClusterClient problemClient = ClusterClientFactory.Factory.Create(
                Properties.Settings.Default.Address, Properties.Settings.Default.Port);

            IAssemblyResolver resolver = new AssemblyResolver();

            var newCore = ComputationalNodeProcessingModuleFactory.Factory.Create
                (resolver.GetProblemNamesPossibleToSolve().ToList());

            var creator = new MessageArrayCreator();

            ComputationalNode computationalNode = new ComputationalNode(statusClient, problemClient, creator, newCore);

            computationalNode.Run();
        }
    }
}
