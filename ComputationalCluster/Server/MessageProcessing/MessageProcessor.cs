using System;
using System.Collections.Generic;
using CommunicationsUtils.Messages;
using Server.Data;
using Server.Interfaces;

namespace Server.MessageProcessing
{
    /// <summary>
    /// So far some processing is same for primary server and backup server, 
    /// will be developed in the future.
    /// It prints some information to console.
    /// </summary>
    public abstract class MessageProcessor : IMessageProcessor
    {
        public virtual void ProcessMessage(Message message, IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            switch (message.MessageType)
            {
                case MessageType.DivideProblemMessage:
                    ProcessDivideProblemMessage(message.Cast<DivideProblem>(), dataSets, activeComponents);
                    break;
                case MessageType.NoOperationMessage:
                    ProcessNoOperationMessage(message.Cast<NoOperation>(), dataSets, activeComponents);
                    break;
                case MessageType.SolvePartialProblemsMessage:
                    ProcessSolvePartialProblemMessage(message.Cast<SolvePartialProblems>(), dataSets, activeComponents);
                    break;
                case MessageType.RegisterMessage:
                    ProcessRegisterMessage(message.Cast<Register>(), dataSets, activeComponents);
                    break;
                case MessageType.RegisterResponseMessage:
                    ProcessRegisterResponseMessage(message.Cast<RegisterResponse>(), dataSets, activeComponents);
                    break;
                case MessageType.SolutionsMessage:
                    ProcessSolutionsMessage(message.Cast<Solutions>(), dataSets, activeComponents);
                    break;
                case MessageType.SolutionRequestMessage:
                    ProcessSolutionRequestMessage(message.Cast<SolutionRequest>(), dataSets, activeComponents);
                    break;
                case MessageType.SolveRequestMessage:
                    ProcessSolveRequestMessage(message.Cast<SolveRequest>(), dataSets, activeComponents);
                    break;
                case MessageType.SolveRequestResponseMessage:
                    ProcessSolveRequestResponseMessage(message.Cast<SolveRequestResponse>(), dataSets, activeComponents);
                    break;
                case MessageType.StatusMessage:
                    ProcessStatusMessage(message.Cast<Status>(), dataSets, activeComponents);
                    break;
                case MessageType.ErrorMessage:
                    ProcessErrorMessage(message.Cast<Error>(), dataSets, activeComponents);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public virtual Message[] CreateResponseMessages(Message message, IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            switch (message.MessageType)
            {
                case MessageType.DivideProblemMessage:
                    return RespondDivideProblemMessage(message.Cast<DivideProblem>(), dataSets, activeComponents);
                case MessageType.NoOperationMessage:
                    return RespondNoOperationMessage(message.Cast<NoOperation>(), dataSets, activeComponents);
                case MessageType.SolvePartialProblemsMessage:
                    return RespondSolvePartialProblemMessage(message.Cast<SolvePartialProblems>(), dataSets, activeComponents);
                case MessageType.RegisterMessage:
                    return RespondRegisterMessage(message.Cast<Register>(), dataSets, activeComponents);
                case MessageType.RegisterResponseMessage:
                    return RespondRegisterResponseMessage(message.Cast<RegisterResponse>(), dataSets, activeComponents);
                case MessageType.SolutionsMessage:
                    return RespondSolutionsMessage(message.Cast<Solutions>(), dataSets, activeComponents);
                case MessageType.SolutionRequestMessage:
                    return RespondSolutionRequestMessage(message.Cast<SolutionRequest>(), dataSets, activeComponents);
                case MessageType.SolveRequestMessage:
                    return RespondSolveRequestMessage(message.Cast<SolveRequest>(), dataSets, activeComponents);
                case MessageType.SolveRequestResponseMessage:
                    return RespondSolveRequestResponseMessage(message.Cast<SolveRequestResponse>(), dataSets, activeComponents);
                case MessageType.StatusMessage:
                    return RespondStatusMessage(message.Cast<Status>(), dataSets, activeComponents);
                case MessageType.ErrorMessage:
                    return RespondErrorMessage(message.Cast<Error>(), dataSets, activeComponents);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void WriteControlInformation(Message message)
        {
            Console.WriteLine("Message is dequeued and is being processed. Message type: " + message.MessageType);
        }

        protected static void WriteResponseMessageControlInformation(Message message, MessageType type)
        {
            Console.WriteLine("Responding {0} message. Returning new {1} message in response.", message.MessageType, type);
        }

        protected virtual void ProcessDivideProblemMessage(DivideProblem message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            WriteControlInformation(message);
        }

        protected virtual void ProcessNoOperationMessage(NoOperation message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            WriteControlInformation(message);
        }

        protected virtual void ProcessSolvePartialProblemMessage(SolvePartialProblems message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            WriteControlInformation(message);
        }

        protected virtual void ProcessRegisterMessage(Register message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            
        }

        protected virtual void ProcessRegisterResponseMessage(RegisterResponse message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            WriteControlInformation(message);
        }

        protected virtual void ProcessSolutionsMessage(Solutions message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            WriteControlInformation(message);
        }

        protected virtual void ProcessSolutionRequestMessage(SolutionRequest message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            WriteControlInformation(message);
        }

        protected virtual void ProcessSolveRequestMessage(SolveRequest message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            WriteControlInformation(message);
        }

        protected virtual void ProcessSolveRequestResponseMessage(SolveRequestResponse message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            WriteControlInformation(message);
        }

        protected virtual void ProcessStatusMessage(Status message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            WriteControlInformation(message);
        }

        protected virtual void ProcessErrorMessage(Error message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            WriteControlInformation(message);
        }

        protected virtual Message[] RespondDivideProblemMessage(DivideProblem message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            return null;
        }

        protected virtual Message[] RespondNoOperationMessage(NoOperation message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            return null;
        }

        protected virtual Message[] RespondSolvePartialProblemMessage(SolvePartialProblems message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            return null;
        }

        protected virtual Message[] RespondRegisterMessage(Register message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            return null;
        }

        protected virtual Message[] RespondRegisterResponseMessage(RegisterResponse message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            return null;
        }

        protected virtual Message[] RespondSolutionsMessage(Solutions message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            return null;
        }

        protected virtual Message[] RespondSolutionRequestMessage(SolutionRequest message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            return null;
        }

        protected virtual Message[] RespondSolveRequestMessage(SolveRequest message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            return null;
        }

        protected virtual Message[] RespondSolveRequestResponseMessage(SolveRequestResponse message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            return null;
        }

        protected virtual Message[] RespondStatusMessage(Status message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            return null;
        }

        protected virtual Message[] RespondErrorMessage(Error message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            return null;
        }
    }
}