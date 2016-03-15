using System;
using System.Collections.Generic;
using CommunicationsUtils.Shared;

namespace Client
{
    public class ArgumentParserExtensions
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