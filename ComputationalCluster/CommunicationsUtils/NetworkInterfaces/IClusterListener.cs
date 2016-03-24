using CommunicationsUtils.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationsUtils.NetworkInterfaces
{
    public interface IClusterListener
    {
        void Start();
        Message[] WaitForRequest();
        string ExtractSocketAddress();
        void SendResponse(Message[] responses);
        void Stop();
    }
}
