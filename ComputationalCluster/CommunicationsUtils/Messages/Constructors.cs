using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationsUtils.Messages
{
    public partial class DivideProblem : Message
    {
        public DivideProblem() : base(MessageType.DivideProblemMessage)
        {
        }
    }

    public partial class NoOperation : Message
    {
        public NoOperation() : base(MessageType.NoOperationMessage)
        {
        }
    }

    public partial class SolutionRequest : Message
    {
        public SolutionRequest() : base(MessageType.SolutionRequestMessage)
        {
        }
    }

    public partial class Register : Message
    {
        public Register() : base(MessageType.RegisterMessage)
        {
        }
    }

    public partial class SolvePartialProblems : Message
    {
        public SolvePartialProblems() : base(MessageType.SolvePartialProblemsMessage)
        {
        }
    }

    public partial class RegisterResponse : Message
    {
        public RegisterResponse() : base(MessageType.RegisterResponseMessage)
        {
        }
    }

    public partial class Solutions : Message
    {
        public Solutions() : base(MessageType.SolutionsMessage)
        {
        } 
    }

    public partial class SolveRequest : Message
    {
        public SolveRequest() : base(MessageType.SolveRequestMessage)
        {
        }
    }

    public partial class SolveRequestResponse : Message
    {
        public SolveRequestResponse() : base(MessageType.SolveRequestResponseMessage)
        {
        }
    }

    public partial class Status : Message
    {
        public Status() : base(MessageType.StatusMessage)
        {
        }
    }

    public partial class Error : Message
    {
        public Error() : base(MessageType.ErrorMessage)
        {
        }
    }
}
