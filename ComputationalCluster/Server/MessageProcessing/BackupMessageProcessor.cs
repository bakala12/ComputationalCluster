﻿using System.Collections.Generic;
using CommunicationsUtils.Messages;
using Server.Data;

namespace Server.MessageProcessing
{
    /// <summary>
    /// Message processor for backup server.
    /// Contains implementations for handling different messages that occur in backup server.
    /// </summary>
    public class BackupMessageProcessor : MessageProcessor
    {
        protected override Message[] RespondRegisterResponseMessage(RegisterResponse message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            WriteResponseMessageControlInformation(message, MessageType.NoOperationMessage);
            return new Message[]
            {
                new NoOperation
                {
                    BackupCommunicationServers = new[]
                    {
                        new NoOperationBackupCommunicationServer()
                        {
                            address = "0.0.0.0",
                            port = 8086
                        }
                    }
                }
            };
        }

        protected override Message[] RespondRegisterMessage(Register message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            WriteResponseMessageControlInformation(message, MessageType.NoOperationMessage);
            return new Message[]
            {
                new NoOperation
                {
                    BackupCommunicationServers = new[]
                    {
                        new NoOperationBackupCommunicationServer()
                        {
                            address = "0.0.0.0",
                            port = 8086
                        }
                    }
                }
            };
        }

        protected override Message[] RespondNoOperationMessage(NoOperation message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            WriteResponseMessageControlInformation(message, MessageType.NoOperationMessage);
            return new Message[]
            {
                new NoOperation
                {
                    BackupCommunicationServers = new[]
                    {
                        new NoOperationBackupCommunicationServer()
                        {
                            address = "0.0.0.0",
                            port = 8086
                        }
                    }
                }
            };
        }

        protected override Message[] RespondSolveRequestResponseMessage(SolveRequestResponse message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            WriteResponseMessageControlInformation(message, MessageType.NoOperationMessage);
            return new Message[]
            {
                new NoOperation
                {
                    BackupCommunicationServers = new[]
                    {
                        new NoOperationBackupCommunicationServer()
                        {
                            address = "0.0.0.0",
                            port = 8086
                        }
                    }
                }
            };
        }

        protected override Message[] RespondDivideProblemMessage(DivideProblem message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            WriteResponseMessageControlInformation(message, MessageType.NoOperationMessage);
            return new Message[]
            {
                new NoOperation
                {
                    BackupCommunicationServers = new[]
                    {
                        new NoOperationBackupCommunicationServer()
                        {
                            address = "0.0.0.0",
                            port = 8086
                        }
                    }
                }
            };
        }

        protected override Message[] RespondSolvePartialProblemMessage(SolvePartialProblems message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            WriteResponseMessageControlInformation(message, MessageType.NoOperationMessage);
            return new Message[]
            {
                new NoOperation
                {
                    BackupCommunicationServers = new[]
                    {
                        new NoOperationBackupCommunicationServer()
                        {
                            address = "0.0.0.0",
                            port = 8086
                        }
                    }
                }
            };
        }

        protected override Message[] RespondSolutionsMessage(Solutions message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            WriteResponseMessageControlInformation(message, MessageType.NoOperationMessage);
            return new Message[]
            {
                new NoOperation
                {
                    BackupCommunicationServers = new[]
                    {
                        new NoOperationBackupCommunicationServer()
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
            WriteResponseMessageControlInformation(message, MessageType.SolutionsMessage);
            return new Message[]
            {
                new Solutions
                {
                    Id = 1,
                    Solutions1 = new []
                    {
                        new SolutionsSolution
                        {
                            Type = SolutionsSolutionType.Final,
                            TaskId = 1,
                            Data = null,
                            TaskIdSpecified = false,
                            ComputationsTime = 50000,
                            TimeoutOccured = false
                        }
                    },
                    CommonData = null,
                    ProblemType = "DVRP"
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
                    Solutions1 = new []
                    {
                        new SolutionsSolution
                        {
                            Type = SolutionsSolutionType.Final,
                            TaskId = 1,
                            Data = null,
                            TaskIdSpecified = false,
                            ComputationsTime = 50000,
                            TimeoutOccured = false
                        }
                    },
                    CommonData = null,
                    ProblemType = "DVRP"
                }
            };
        }
    }
}
