using System.Net;
using CommunicationsUtils.Argument_parser;
using CommunicationsUtils.NetworkInterfaces.Factories;
using Server.Extensions;
using Server.Data;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace Server
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var parser = new ArgumentParser(OptionSetPool.ServerOptionsSet);
            parser.ProcessArguments(args);
            parser.UpdateConfiguration(parser.map);
            var  port = Properties.Settings.Default.Port;
            var state = Properties.Settings.Default.IsBackup ? ServerState.Backup : ServerState.Primary;

            var listener = ClusterListenerFactory.Factory.Create(IPAddress.Any, port);
            var client = ClusterClientFactory.Factory.Create(Properties.Settings.Default.MasterAddress,
                Properties.Settings.Default.MasterPort);
            var server = (state == ServerState.Primary)
                ? new ComputationalServer(listener)
                : new ComputationalServer(client, listener);
            server.Run(); //starting server
        }
    }
}
