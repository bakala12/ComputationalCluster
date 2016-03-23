using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CommunicationsUtils.Messages;
using log4net;
using Server.Data;
using Server.Interfaces;

namespace Server.MessageProcessing
{
    //TODO: if methods of primary and backup are the same, let them stay here
    //TODO: some of methods must be redefined in BackupMessageProcessor
    //TODO: some are unique for PrimaryServer
    /// <summary>
    /// Message processor for component.
    /// Contains implementations for handling different messages that occur in component.
    /// </summary>
    public abstract class MessageProcessor : IMessageProcessor
    {

        protected ConcurrentQueue<Message> _synchronizationQueue;

        protected static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public Thread StatusThread { get; protected set; }


        protected MessageProcessor(ConcurrentQueue<Message> synchronizationQueue)
        {
            _synchronizationQueue = synchronizationQueue;
        }

        private void StatusThreadWork (int who, 
            IDictionary<int, ActiveComponent> activeComponents, IDictionary<int, ProblemDataSet> dataSets)
        {
            while (true)
            {
                var elapsed = activeComponents[who].StatusWatch.ElapsedMilliseconds;
                if (elapsed > Properties.Settings.Default.Timeout)
                {
                    Message deregister = new Register()
                    {                 
                        Deregister = true,
                        DeregisterSpecified = true,
                        Id = (ulong) who,
                        IdSpecified = true
                    };
                    _synchronizationQueue.Enqueue(deregister);
                    log.DebugFormat("TIMEOUT of {0}. Deregistering.", activeComponents[who].ComponentType);
                    DataSetOps.HandleClientMalfunction(activeComponents, who, dataSets);
                    activeComponents.Remove(who);
                    return;
                }
                Thread.Sleep((int) Properties.Settings.Default.Timeout);
            }
        }

        protected void RunStatusThread(int who, 
            IDictionary<int, ActiveComponent> activeComponents, IDictionary<int, ProblemDataSet> dataSets)
        {
            var t = new Thread(() => StatusThreadWork(who, activeComponents, dataSets));
            t.Start();
            StatusThread = t;
        }

        /// <summary>
        /// Processes message.
        /// </summary>
        /// <param name="message">Instance of message to process</param>
        /// <param name="dataSets">Dictionary of problem data sets (maybe to update one of these or maybe not)</param>
        /// <param name="activeComponents">Dictionary of active components (maybe to update one of these or maybe not)</param>
        /// <param name="backups">backups list</param>
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
        /// <param name="backups">backups' list</param>
        /// <returns></returns>
        public virtual Message[] CreateResponseMessages(Message message, IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents, List<BackupServerInfo> backups)
        {
            switch (message.MessageType)
            {
                case MessageType.DivideProblemMessage:
                    return RespondDivideProblemMessage(message.Cast<DivideProblem>(), dataSets, activeComponents);
                case MessageType.NoOperationMessage:
                    return RespondNoOperationMessage(message.Cast<NoOperation>(), dataSets, activeComponents);
                case MessageType.SolvePartialProblemsMessage:
                    return RespondSolvePartialProblemMessage(message.Cast<SolvePartialProblems>(), dataSets, activeComponents, backups);
                case MessageType.RegisterMessage:
                    return RespondRegisterMessage(message.Cast<Register>(), dataSets, activeComponents, backups);
                case MessageType.RegisterResponseMessage:
                    return RespondRegisterResponseMessage(message.Cast<RegisterResponse>(), dataSets, activeComponents);
                case MessageType.SolutionsMessage:
                    return RespondSolutionsMessage(message.Cast<Solutions>(), dataSets, activeComponents, backups);
                case MessageType.SolutionRequestMessage:
                    return RespondSolutionRequestMessage(message.Cast<SolutionRequest>(), dataSets, activeComponents, backups);
                case MessageType.SolveRequestMessage:
                    return RespondSolveRequestMessage(message.Cast<SolveRequest>(), dataSets, activeComponents, backups);
                case MessageType.StatusMessage:
                    return RespondStatusMessage(message.Cast<Status>(), dataSets, activeComponents, backups);
                case MessageType.ErrorMessage:
                    return RespondErrorMessage(message.Cast<Error>(), dataSets, activeComponents);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void WriteControlInformation(Message message)
        {
            log.DebugFormat("Message is dequeued and is being processed. Message type: " + message.MessageType);
        }

        protected static void WriteResponseMessageControlInformation(Message message, MessageType type)
        {
            log.DebugFormat("Responding {0} message. Returning new {1} message in response.", message.MessageType, type);
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
            //nothing. noOperation is not enqueued anywhere
            //TODO: Update Backup list on server!
        }

        protected virtual void ProcessSolvePartialProblemMessage(SolvePartialProblems message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            WriteControlInformation(message);
            //TODO: implement it on backup side
            //update dataset for given problemId
            //message from TM and only from it, so set partialSets array (it will be enough)
            if (!dataSets.ContainsKey((int)message.Id))
                return;
            int id = (int) message.Id;
            dataSets[id].PartialSets = new PartialSet[message.PartialProblems.Length];
            for (int i = 0; i < message.PartialProblems.Length; i++)
            {
                dataSets[id].PartialSets[i] = new PartialSet()
                {
                    NodeId = 0,
                    PartialSolution = null,
                    PartialProblem = message.PartialProblems[i],
                    Status = PartialSetStatus.Fresh
                };
            }
        }

        protected virtual void ProcessRegisterMessage(Register message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            //TODO: message delivered to backup - update active components structure
            //TODO: can also be deregister message
        }

        protected virtual void ProcessRegisterResponseMessage(RegisterResponse message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            WriteControlInformation(message);
            //nothing. it is not enqueued
        }

        protected virtual void ProcessSolutionsMessage(Solutions message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            //TODO: implement it on backup side
            WriteControlInformation(message);
            //message delivered from TM or CN
            //in case of TM - it is final solution. adjust dataset for proper problemId
            //that means, just make only one partialSet with solutions as given from Solutions message
            //in case of CN - it is partial solution. adjust partialSet array element (for taskId) 
            //in proper problemId
            if (message.Solutions1 == null || message.Solutions1.Length == 0)
                return;

            int key = (int)message.Id;
            if (!dataSets.ContainsKey(key))
                return;
            //this is from TM:
            if (message.Solutions1.Length == 1 && message.Solutions1[0].Type == SolutionsSolutionType.Final)
            {
                dataSets[key].PartialSets = new PartialSet[1];
                dataSets[key].PartialSets[0] = new PartialSet()
                {
                    NodeId = 0,
                    PartialProblem = null,
                    PartialSolution = message.Solutions1[0],
                    Status = PartialSetStatus.Sent
                };
            }
            //this is from CN
            else
            {
                //only one solution is delivered by CN at a time
                if (message.Solutions1.Length != 1)
                    return;
                var taskId = message.Solutions1[0].TaskId;
                foreach (var partialSet in dataSets[key].PartialSets)
                {
                    if (partialSet.PartialProblem.TaskId == taskId)
                    {
                        partialSet.PartialSolution = message.Solutions1[0];
                        partialSet.Status = PartialSetStatus.Ongoing;
                        break;
                    }
                }
            }
        }

        protected virtual void ProcessSolutionRequestMessage(SolutionRequest message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            WriteControlInformation(message);
            //nothing. this message is not enqueued (response is immediate)
        }

        protected virtual void ProcessSolveRequestMessage(SolveRequest message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            WriteControlInformation(message);
            //TODO: implement it on backup side (with id specified)
        }

        protected virtual void ProcessSolveRequestResponseMessage(SolveRequestResponse message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            WriteControlInformation(message);
            //nothing. not processed anywhere
        }

        protected virtual void ProcessStatusMessage(Status message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            WriteControlInformation(message);
            //nothing. status is not enqueued
        }

        protected virtual void ProcessErrorMessage(Error message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            WriteControlInformation(message);
            //error shouldn't be enqueued, so nothing
        }

        protected virtual Message[] RespondDivideProblemMessage(DivideProblem message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            return null;
        }

        protected abstract Message[] RespondNoOperationMessage(NoOperation message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents);

        protected abstract Message[] RespondSolvePartialProblemMessage(SolvePartialProblems message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents, List<BackupServerInfo> backups);

        protected abstract Message[] RespondRegisterMessage(Register message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents, List<BackupServerInfo> backups);

        protected abstract Message[] RespondRegisterResponseMessage(RegisterResponse message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents);

        protected abstract Message[] RespondSolutionsMessage(Solutions message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents, List<BackupServerInfo> backups);

        protected abstract Message[] RespondSolutionRequestMessage(SolutionRequest message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents, List<BackupServerInfo> backups);

        protected virtual Message[] RespondSolveRequestMessage(SolveRequest message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents, List<BackupServerInfo> backups)
        {
            //sent by client node. create new issue in dataset with unique problemId,
            //send back NoOp + SolveRequestResponse with proper problemId
            int maxProblemId = dataSets.Count == 0 ? 1 : dataSets.Keys.Max() + 1;
            var newSet = new ProblemDataSet()
            {
                CommonData = message.Data,
                PartialSets = null,
                ProblemType = message.ProblemType,
                TaskManagerId = 0
            };
            dataSets.Add(maxProblemId, newSet);
            log.DebugFormat("New problem, ProblemType={0}. Assigned id: {1}", 
                message.ProblemType, maxProblemId);
            log.DebugFormat("New problem, ProblemType={0}. Assigned id: {1}",
                message.ProblemType, maxProblemId);

            message.Id = (ulong) maxProblemId;
            message.IdSpecified = true;
            _synchronizationQueue.Enqueue(message);

            return new Message[]
            {
                new NoOperation()
                {
                    BackupServersInfo = backups.ToArray()
                },
                new SolveRequestResponse()
                {
                    Id = (ulong) maxProblemId
                }
            };
        }

        protected abstract Message[] RespondStatusMessage(Status message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents, List<BackupServerInfo> backups);
            

        protected virtual Message[] RespondErrorMessage(Error message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            //practically nothing to do
            //warn logger, print something on console if verbose
            log.DebugFormat("Error message acquired. Type={0}, Message={1}",
                message.ErrorType, message.ErrorMessage);
            log.DebugFormat("Error message acquired. Type={0}, Message={1}",
                message.ErrorType, message.ErrorMessage);
            return null;
        }
    }
}