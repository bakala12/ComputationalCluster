using CommunicationsUtils.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationsUtils.ClientComponentCommon
{
    /// <summary>
    /// creates array of messages from params messages
    /// provides much more test-friendly code
    /// </summary>
    public interface IMessageArrayCreator
    {
        Message[] Create(params Message[] messages);
    }
}
