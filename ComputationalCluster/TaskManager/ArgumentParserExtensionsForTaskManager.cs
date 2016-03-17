﻿using System;
using System.Collections.Generic;
using CommunicationsUtils.Argument_parser;
using CommunicationsUtils.Shared;

namespace TaskManager
{
    public static class ArgumentParserExtensionsForTaskManager
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
                    default:
                        throw new ArgumentException();
                }
            }
        }
    }
}