using System;
using System.IO;
using System.Net;
using CommunicationsUtils.Argument_parser;
using CommunicationsUtils.NetworkInterfaces.Factories;
using log4net;
using Server.Extensions;
using Server.Data;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace Server
{
    internal class Program
    {
        /// <summary>
        /// Even though we do not use logger in this class, there is a need to instantiate logger to set -verbose logging to console from starting parameters
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
                : new ComputationalServer(client);
            server.Run(); //starting server
        }

        private static string GetPublicIp()
        {
            string direction;
            var request = WebRequest.Create("http://checkip.dyndns.org/");
            using (var response = request.GetResponse())
                // ReSharper disable once AssignNullToNotNullAttribute
            using (var stream = new StreamReader(response.GetResponseStream()))
            {
                direction = stream.ReadToEnd();
            }

            //Search for the ip in the html
            var first = direction.IndexOf("Address: ", StringComparison.Ordinal) + 9;
            var last = direction.LastIndexOf("</body>", StringComparison.Ordinal);
            direction = direction.Substring(first, last - first);
            log.Debug("Public ip of this machine is: " + direction);
            return direction;
        }
    }
}
