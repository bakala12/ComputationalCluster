using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicationsUtils.Messages;

namespace CommunicationsUtils.ClientComponentCommon
{
    public class MessageArrayCreator : IMessageArrayCreator
    {
        
        public Message[] Create(params Message[] messages)
        {
            Message[] array = new Message[messages.Length];
            messages.CopyTo(array, 0);
            return array;
        }
    }
}
