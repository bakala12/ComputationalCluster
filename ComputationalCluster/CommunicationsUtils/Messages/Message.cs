using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationsUtils.Messages
{
    public abstract class Message
    {
        protected Message(MessageType type)
        {
            Type = type;
        }

        [System.Xml.Serialization.XmlIgnore]
        public MessageType Type { get; } 
    }

    public enum MessageType
    {
        DivideProblemMessage,
        NoOperationMessage,
        SolvePartialProblemsMessage,
        RegisterMessage, 
        RegisterResponseMessage, 
        SolutionsMessage,
        SolutionRequestMessage,
        SolveRequestMessage,
        SolveRequestResponseMessage,
        StatusMessage
    }
}
