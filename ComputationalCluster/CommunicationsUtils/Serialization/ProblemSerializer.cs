using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using AlgorithmSolvers.DVRPEssentials;
using CommunicationsUtils.Messages;

namespace CommunicationsUtils.Serialization
{
    public class ProblemSerializer
    {
        public DVRPProblemInstance FromXmlString(string xml)
        {
            StringReader stream = new StringReader(xml);
            XmlReader reader = XmlReader.Create(stream);
            var node = reader.MoveToContent();
            var serializer =new XmlStringSerializer<DVRPProblemInstance>();
            return serializer.FromXmlString(xml);
        }

        public string ToXmlString(DVRPProblemInstance problem)
        {
            var serializer = new XmlStringSerializer<DVRPProblemInstance>();
            return serializer.ToXmlString(problem);
        }
    }
}
