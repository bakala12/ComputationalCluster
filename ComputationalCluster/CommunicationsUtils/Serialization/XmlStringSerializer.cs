using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CommunicationsUtils.Serialization
{
    public class XmlStringSerializer<T> where T : class
    {
        private XmlSerializer _serializer = new XmlSerializer(typeof(T));

        public virtual string ToXmlString(T message)
        {
            using (var sw = new StringWriter())
            {
                _serializer.Serialize(sw, message);
                return sw.ToString();       
            }
        }

        public virtual T FromXmlString(string xml)
        {
            using (var sw = new StringReader(xml))
            {
                return _serializer.Deserialize(sw) as T;
            }
        }
    }
}
