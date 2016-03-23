using System;
using System.Collections.Generic;
using CommunicationsUtils.Argument_parser;
using CommunicationsUtils.Log4Net;
using CommunicationsUtils.Shared;

namespace Client
{
    public static class ArgumentParserExtensionsForClient
    {
        public static void UpdateConfiguration(this ArgumentParser parser, Dictionary<string, string> map)
        {
            foreach (var pair in map)
            {
                switch (pair.Key)
                {
                    case "port=":
                        Properties.Settings.Default.Port = pair.Value.ChangeType<int>();
                        break;
                    case "address=":
                        Properties.Settings.Default.Address = pair.Value;
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