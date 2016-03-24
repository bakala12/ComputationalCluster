using System;
using CommunicationsUtils.Argument_parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class ArgumentParserTests
    {
        [TestMethod]
        public void TestArguments_ForSampleServerExecution()
        {
            var parser = new ArgumentParser(OptionSetPool.ServerOptionsSet);
            var args = new[] {"NazwaProgramu", "-port", "4","-backup", "-maddress", "10.10.10.10", "-mport", "8086", "-time", "1000"};
            parser.ProcessArguments(args);
            Assert.AreEqual("4", parser.map["port="]);
            Assert.AreEqual("8086", parser.map["mport="]);
            Assert.AreEqual("1000", parser.map["time="]);

        }

        [TestMethod]
        public void TestArguments_ForAnotherServerExecution()
        {
            var parser = new ArgumentParser(OptionSetPool.ServerOptionsSet);
            var args = new[] { "NazwaProgramu", "-port", "4", "-time", "1000" };
            parser.ProcessArguments(args);
            Assert.AreEqual("4", parser.map["port="]);
            Assert.AreEqual("1000", parser.map["time="]);
        }

        [TestMethod]
        public void TestArguments_ForSampleClientExecution()
        {
            var parser = new ArgumentParser(OptionSetPool.ClientOptionsSet);
            var args = new[] { "NazwaProgramu", "-address", "1000", "-port", "4"};
            parser.ProcessArguments(args);
            Assert.AreEqual("1000", parser.map["address="]);
            Assert.AreEqual("4", parser.map["port="]);
        }

        [TestMethod]
        public void TestVerboseArg_ForServerExecution()
        {
            var parser = new ArgumentParser(OptionSetPool.ServerOptionsSet);
            var args = new[] { "NazwaProgramu", "-verbose" };
            parser.ProcessArguments(args);
            Assert.AreEqual("verbose", parser.map["verbose"]);
        }
    }
}
