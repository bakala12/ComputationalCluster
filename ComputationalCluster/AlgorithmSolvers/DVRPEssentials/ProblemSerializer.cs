using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using AlgorithmSolvers.DVRPEssentials;
using CommunicationsUtils.Serialization;

namespace AlgorithmSolvers.DVRPEssentials
{
    public class ProblemSerializer
    {
        public IProblemInstance FromXmlString(string xml)
        {
            StringReader stream = new StringReader(xml);
            XmlReader reader = XmlReader.Create(stream);
            var node = reader.MoveToContent();
            string s = reader.LocalName;
            var problemType = typeof(IProblemInstance).Assembly.GetType("AlgorithmSolvers.DVRPEssentials." + s);
            var type = typeof(XmlStringSerializer<>).MakeGenericType(problemType);
            var serializer = Activator.CreateInstance(type);
            var method = type.GetMethod("FromXmlString");
            return (IProblemInstance)method.Invoke(serializer, new object[] { xml });
        }

        public string ToXmlString(IProblemInstance problem)
        {
            var serializer = typeof(XmlStringSerializer<>).MakeGenericType(problem.GetType());
            var method = serializer.GetMethod("ToXmlString");
            var ser = Activator.CreateInstance(serializer);
            return (string)method.Invoke(ser, new object[] { problem });
        }
    }
}
