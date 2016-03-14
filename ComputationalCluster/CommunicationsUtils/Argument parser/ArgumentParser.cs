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
        public Dictionary<string, string> map { get; } = new Dictionary<string, string>();
        public OptionSet options { get; }

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
        }
    }
}