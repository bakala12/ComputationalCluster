using System;
using System.Net;
using CommunicationsUtils.Argument_parser;
using CommunicationsUtils.NetworkInterfaces.Factories;
using Server.Extensions;
using Server.Data;

namespace Server
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var parser = new ArgumentParser(OptionSetPool.ServerOptionsSet);
            parser.ProcessArguments(args);
            parser.UpdateConfiguration(parser.map);

            var address = Properties.Settings.Default.Address;
            var port = Properties.Settings.Default.Port;
            var state = Properties.Settings.Default.IsBackup ? ServerState.Backup : ServerState.Primary;

            IPAddress ipAddress;
            if (!IPAddress.TryParse(address, out ipAddress))
            {
                throw new Exception("Invalid ip address");
            }
            var listener = ClusterListenerFactory.Factory.Create(IPAddress.Any, port);
            var server = new ComputationalServer(listener, state); 
            server.Run();
        }
    }
}
