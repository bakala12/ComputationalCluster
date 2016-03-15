using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationsUtils.Messages
{
    [System.Xml.Serialization.XmlInclude(typeof(Status))]
    public abstract class Message
    {
        protected Message(MessageType messageType)
        {
            MessageType = messageType;
        }

        [System.Xml.Serialization.XmlIgnore]
        public MessageType MessageType { get; } 

        public T Cast<T> () where T : Message
        {
            return (T)this;
        }
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
        StatusMessage,
        ErrorMessage
    }
}
