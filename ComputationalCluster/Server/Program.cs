using System;
using System.IO;
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
            Console.WriteLine("Public ip of this machine is: " + direction);
            return direction;
        }
    }
}
