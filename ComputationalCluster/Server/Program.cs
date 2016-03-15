using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CommunicationsUtils.NetworkInterfaces;
using CommunicationsUtils.NetworkInterfaces.Factories;
using Server.Data;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            string address = Properties.Settings.Default.Address;
            int port = Properties.Settings.Default.Port;
            ServerState state = Properties.Settings.Default.IsBackup ? ServerState.Backup : ServerState.Primary;
            //arg parsing
            //fell free here to override default values here

            IPAddress ipAddress;
            if(!IPAddress.TryParse(address, out ipAddress))
                throw new Exception("Invalid ip address");
            IClusterListener listener = ClusterListenerFactory.Factory.Create(ipAddress, port);
            var server = new ComputationalServer(listener, state); 
            server.Run();
        }
    }
}
