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
using log4net;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace Client
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

            IClusterClient clusterClient = ClusterClientFactory.Factory.Create(
                Properties.Settings.Default.Address, Properties.Settings.Default.Port);
            var core = ClientNodeProcessingModuleFactory.Factory.Create();
            IMessageArrayCreator creator = new MessageArrayCreator();

            ClientNode clientNode = new ClientNode(clusterClient, core, creator);
            clientNode.Run();
        }
    }
}
