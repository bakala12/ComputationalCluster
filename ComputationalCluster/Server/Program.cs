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

            var address = GetPublicIp();
            var  port = Properties.Settings.Default.Port;
            var state = Properties.Settings.Default.IsBackup ? ServerState.Backup : ServerState.Primary;

            IPAddress ipAddress;
            if (!IPAddress.TryParse(address, out ipAddress))
            {
                throw new Exception("Invalid ip address");
            }
            var listener = ClusterListenerFactory.Factory.Create(ipAddress, port);
            var server = new ComputationalServer(listener, state); 
            server.Run();
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
