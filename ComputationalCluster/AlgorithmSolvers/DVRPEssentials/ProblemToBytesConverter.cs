using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgorithmSolvers.DVRPEssentials
{
    public class ProblemToBytesConverter
    {
        private readonly ProblemSerializer _serializer = new ProblemSerializer();

        public byte[] ToByteArray(IProblemInstance problem)
        {
            return Encoding.UTF8.GetBytes(_serializer.ToXmlString(problem));
        }

        public IProblemInstance FromBytesArray(byte[] bytes)
        {
            return _serializer.FromXmlString(Encoding.UTF8.GetString(bytes));
        }
    }
}
