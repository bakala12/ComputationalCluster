using System;
using System.Collections.Generic;
using CommunicationsUtils.Messages;
using Server.Data;
using Server.Interfaces;

namespace Server.MessageProcessing
{
    /// <summary>
    /// Message processor for component.
    /// Contains implementations for handling different messages that occur in component.
    /// </summary>
    public abstract class MessageProcessor : IMessageProcessor
    {

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Processes message.
        /// </summary>
        /// <param name="message">Instance of message to process</param>
        /// <param name="dataSets">Dictionary of problem data sets (maybe to update one of these or maybe not)</param>
        /// <param name="activeComponents">Dictionary of active components (maybe to update one of these or maybe not)</param>
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

        /// <summary>
        /// Creates array of response messages for specified message.
        /// </summary>
        /// <param name="message">Instance of message to create response messages for</param>
        /// <param name="dataSets">Dictionary of problem data sets (maybe to update one of these or maybe not)</param>
        /// <param name="activeComponents">Dictionary of active components (maybe to update one of these or maybe not)</param>
        /// <returns></returns>
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
            log.Debug(string.Format("Message is dequeued and is being processed. Message type: " + message.MessageType));
        }

        protected static void WriteResponseMessageControlInformation(Message message, MessageType type)
        {
            log.Debug(string.Format("Responding {0} message. Returning new {1} message in response.", message.MessageType, type));
        }

        protected virtual void ProcessDivideProblemMessage(DivideProblem message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            WriteControlInformation(message);
            //TODO: message delivered to backup only - update proper data set
        }

        protected virtual void ProcessNoOperationMessage(NoOperation message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            WriteControlInformation(message);
            //TODO: delete it. nooperation is not enqueued
        }

        protected virtual void ProcessSolvePartialProblemMessage(SolvePartialProblems message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            WriteControlInformation(message);
            //TODO: update dataset for given problemId
            //TODO: message from TM and only from it, so set partialSets array (it will be enough)
        }

        protected virtual void ProcessRegisterMessage(Register message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            //TODO: delete it. register msg is not processed
        }

        protected virtual void ProcessRegisterResponseMessage(RegisterResponse message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            WriteControlInformation(message);
            //TODO: message delivered to backup only - update active components structure
        }

        protected virtual void ProcessSolutionsMessage(Solutions message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            WriteControlInformation(message);
            //TODO: message delivered from TM or CN
            //TODO: in case of TM - it is final solution. adjust dataset for proper problemId
            //TODO: that means, just make only one partialSet with solutions as given from Solutions message
            //TODO: in case of CN - it is partial solution. adjust partialSet array element (for taskId) 
            //TODO: in proper problemId
        }

        protected virtual void ProcessSolutionRequestMessage(SolutionRequest message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            WriteControlInformation(message);
            //TODO: delete it. this message is not enqueued (response is immediate)
        }

        protected virtual void ProcessSolveRequestMessage(SolveRequest message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            WriteControlInformation(message);
            //TODO: delete it. this message is not enqueued (response is immediate)
        }

        protected virtual void ProcessSolveRequestResponseMessage(SolveRequestResponse message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            WriteControlInformation(message);
            //TODO: i don't know, delete it? this message shouldn't be delivered anywhere but to Client node, I think
            //TODO: not even to backup
        }

        protected virtual void ProcessStatusMessage(Status message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            WriteControlInformation(message);
            //TODO: delete it. status shouldn't be enqueued
        }

        protected virtual void ProcessErrorMessage(Error message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            WriteControlInformation(message);
            //TODO: for now, nothing here. specification is not specified yet
        }

        protected virtual Message[] RespondDivideProblemMessage(DivideProblem message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            //TODO: obsolete. divide problem comes to backup server only, and backup doesnt respond nodes (only
            //TODO: other backups)
            return null;
        }

        protected virtual Message[] RespondNoOperationMessage(NoOperation message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            //TODO: delete. noOperation is not enqueued
            return null;
        }

        protected virtual Message[] RespondSolvePartialProblemMessage(SolvePartialProblems message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            //TODO: sent by TM. send noOperation only.
            return null;
        }

        protected virtual Message[] RespondRegisterMessage(Register message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            //TODO: add new entity to ActiveComponents, create immediately registerResponse message with this id
            return null;
        }

        protected virtual Message[] RespondRegisterResponseMessage(RegisterResponse message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            //TODO: delete it. same reason as RespondDivideProblemMessage
            return null;
        }

        protected virtual Message[] RespondSolutionsMessage(Solutions message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            //TODO: sent by CN or TM. send NoOperation only.
            return null;
        }

        protected virtual Message[] RespondSolutionRequestMessage(SolutionRequest message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            //TODO: sent by client node. send NoOperation + CaseExtractor.GetSolutionState
            return null;
        }

        protected virtual Message[] RespondSolveRequestMessage(SolveRequest message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            //TODO: sent by client node. create new issue in dataset with unique problemId,
            //TODO: send back NoOp + SolveRequestResponse with proper problemId
            return null;
        }

        protected virtual Message[] RespondSolveRequestResponseMessage(SolveRequestResponse message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            //TODO: delete it. same reason as RespondDivideProblemMessage
            return null;
        }

        protected virtual Message[] RespondStatusMessage(Status message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            //TODO: (in second stage I think) reset timeout watch for this componentId
            //TODO: if sent by TM - send NoOp + return from CaseExtractor.GetMessageForTaskManager
            //TODO: if sent by CN - send NoOp + return from CaseExtractor.GetMessageForCompNode
            //TODO: if sent by backup - we don't know yet what to send, probably whole Synchronization Queue
            return null;
        }

        protected virtual Message[] RespondErrorMessage(Error message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            //TODO: practically nothing to do - specification is not specified yet
            return null;
        }
    }
}