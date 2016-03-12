using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationsUtils.Miscellaneous
{
    public static class IPAddressEx
    {
        /// <summary>
        /// extension for IPAddress class (supporting exceptions)
        /// </summary>
        /// <param name="addr">object on that method is called</param>
        /// <param name="ipString">string to construct ipaddress</param>
        /// <returns>new instance of ipaddress</returns>
        public static IPAddress FromString(this IPAddress addr, string ipString)
        {
            IPAddress _addr;
            if (!IPAddress.TryParse(ipString, out _addr))
            {
                throw new Exception("Wrong ip string!");
            }
            return _addr;
        }
    }
}
