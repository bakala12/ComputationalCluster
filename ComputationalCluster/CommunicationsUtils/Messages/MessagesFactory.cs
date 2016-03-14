using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationsUtils.Messages
{
    public static class MessagesFactory
    {
        public static Message CreateEmptyMessage(MessageType type)
        {
            switch (type)
            {
                case MessageType.DivideProblemMessage:
                    return new DivideProblem();
                case MessageType.NoOperationMessage:
                    return new NoOperation();
                case MessageType.SolvePartialProblemsMessage:
                    return new SolvePartialProblems();
                case MessageType.RegisterMessage:
                    return new Register();
                case MessageType.RegisterResponseMessage:
                    return new RegisterResponse();
                case MessageType.SolutionsMessage:
                    return new Solutions();
                case MessageType.SolutionRequestMessage:
                    return new SolutionRequest();
                case MessageType.SolveRequestMessage:
                    return new SolveRequest();
                case MessageType.SolveRequestResponseMessage:
                    return new SolveRequestResponse();
                case MessageType.StatusMessage:
                    return new Status();
                case MessageType.ErrorMessage:
                    return new Error();
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }
    }
}
