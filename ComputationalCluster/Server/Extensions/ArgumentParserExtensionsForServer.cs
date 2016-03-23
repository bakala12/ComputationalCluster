using System;
using System.Collections.Generic;
using CommunicationsUtils.Argument_parser;
using CommunicationsUtils.Log4Net;
using CommunicationsUtils.Shared;

namespace Server.Extensions
{
    /// <summary>
    /// Extension methods class for argument parser class.
    /// </summary>
    public static class ArgumentParserExtensionsForServer
    {
        /// <summary>
        /// Extension method for configuration update.
        /// </summary>
        /// <param name="parser">Instance of argument parser</param>
        /// <param name="map">Dictionary of key value pair of option and assigned value to it</param>
        public static void UpdateConfiguration(this ArgumentParser parser, Dictionary<string, string> map)
        {
            foreach (var pair in map)
            {
                switch (pair.Key)
                { 
                    case "port=":
                        Properties.Settings.Default.Port = pair.Value.ChangeType<int>();
                        break;
                    case "time=":
                        Properties.Settings.Default.Timeout = pair.Value.ChangeType<uint>();
                        break;
                    case "backup":
                        Properties.Settings.Default.IsBackup = true;
                        break;
                    case "maddress=":
                        Properties.Settings.Default.MasterAddress = pair.Value;
                        break;
                    case "mport=":
                        Properties.Settings.Default.MasterPort = pair.Value.ChangeType<int>();
                        break;
                    case "verbose":
                        LogHelper.EnableConsoleLogging();
                        break;
                    default:
                        throw new ArgumentException();
                }
            }
        }    
    }
}