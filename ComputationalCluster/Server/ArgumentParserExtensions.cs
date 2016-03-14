using System;
using System.Collections.Generic;
using CommunicationsUtils.Shared;
namespace Server
{
    public static class ArgumentParserExtensionsForServer
    {
        public static void UpdateConfiguration(Dictionary<string, string> map)
        {
            foreach (var pair in map)
            {
                switch (pair.Key)
                {
                    case "port=":
                        Properties.Settings.Default.Port = pair.Value.ChangeType<int>();
                        break;
                    case "time=":
                        Properties.Settings.Default.Timeout = pair.Value.ChangeType<long>();
                        break;
                    case "backup":
                        Properties.Settings.Default.IsBackup = pair.Value.ChangeType<bool>();
                        break;
                    case "maddress=":
                        Properties.Settings.Default.MasterAddress = pair.Value;
                        break;
                    case "mport=":
                        Properties.Settings.Default.MasterPort = pair.Value.ChangeType<int>();
                        break;
                    case "address=":
                        Properties.Settings.Default.Address = pair.Value;
                        break;
                    default:
                        throw new ArgumentException();
                }
            }
        }    
    }
}