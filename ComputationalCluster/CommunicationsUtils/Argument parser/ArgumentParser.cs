using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.InteropServices;
using NDesk.Options;
// ReSharper disable InconsistentNaming

namespace CommunicationsUtils.Argument_parser
{
    public class ArgumentParser
    {
        private readonly Dictionary<string, string> map = new Dictionary<string, string>();
        private readonly OptionSet options;

        public ArgumentParser(IEnumerable<string> options)
        {
            this.options = new CustomOptionSet();
            foreach (var o in options)
            {
                Action<string> action = s =>
                {
                    map.Add(o, s);
                };
                this.options.Add(o, action);
            }
        }

        public void ProcessArguments(string[] args)
        {
            options.Parse(args);
            var keys = new List<string>(map.Keys);
            keys.Sort();
            //TODO
            foreach (var key in keys)
            {
                Console.WriteLine("Key: {0}={1}", key, map[key]);
            }
        }
    }
}