using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationsUtils.NetworkInterfaces
{
    public interface IClusterClient
    {
        Messages.Message[] SendRequests(Messages.Message[] requests);
    }
}
