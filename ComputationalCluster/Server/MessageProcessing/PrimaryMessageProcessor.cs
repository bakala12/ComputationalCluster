using System.Collections.Generic;
using CommunicationsUtils.Messages;
using Server.Data;

namespace Server.MessageProcessing
{
    /// <summary>
    /// Lets say that there are all messages that main server would send response.
    /// So far it returns proper messages.
    /// It also prints some information to console.
    /// </summary>
    public class PrimaryMessageProcessor : MessageProcessor
    {
        protected override Message[] RespondRegisterMessage(Register message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            WriteResponseMessageControlInformation(message, MessageType.RegisterMessage);
            return new Message[]
            {
                new RegisterResponse()
                {
                    Timeout = Properties.Settings.Default.Timeout,
                    Id = 1,
                    BackupCommunicationServers = new []
                    {
                        new RegisterResponseBackupCommunicationServer
                        {
                            address = "0.0.0.0",
                            port = 8086
                        }
                    }
                }
            };
        }

        protected override Message[] RespondStatusMessage(Status message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            WriteResponseMessageControlInformation(message, MessageType.NoOperationMessage);
            return new Message[]
            {
                new NoOperation
                {
                    BackupCommunicationServers =  new []
                    {
                        new NoOperationBackupCommunicationServer
                        {
                            address = "0.0.0.0",
                            port = 8086
                        }
                    }
                }
            };
        }

        protected override Message[] RespondSolutionRequestMessage(SolutionRequest message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            WriteResponseMessageControlInformation(message, MessageType.SolutionsMessage);
            return new Message[]
            {
                new Solutions
                {
                    Id = 1,
                    CommonData = null,
                    ProblemType = "DVRP",
                    Solutions1 = new []
                    {
                        new SolutionsSolution
                        {
                            TimeoutOccured = false,
                            TaskIdSpecified = false,
                            TaskId = 1,
                            Data = null,
                            ComputationsTime = 50000,
                            Type = SolutionsSolutionType.Final
                        }
                    }
                }
            };
        }

        protected override Message[] RespondSolveRequestMessage(SolveRequest message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            WriteResponseMessageControlInformation(message, MessageType.SolveRequestResponseMessage);
            return new Message[]
            {
                new SolveRequestResponse
                {
                    Id = 1
                } 
            };
        }

        protected override Message[] RespondErrorMessage(Error message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            // TODO
            WriteResponseMessageControlInformation(message, MessageType.ErrorMessage);
            return new Message[]
            {
                message
            };
        }
    }
}
