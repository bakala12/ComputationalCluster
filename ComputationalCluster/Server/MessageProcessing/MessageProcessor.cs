using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CommunicationsUtils.Messages;
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

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private List<Thread> _currentlyWorkingThreads;

        public MessageProcessor(List<Thread> currentlyWorkingThreads)
        {
            _currentlyWorkingThreads = currentlyWorkingThreads;
        }

        private static void StatusThreadWork (int who, 
            IDictionary<int, ActiveComponent> activeComponents, IDictionary<int, ProblemDataSet> dataSets)
        {
            while (true)
            {
                var elapsed = activeComponents[who].StatusWatch.ElapsedMilliseconds;
                if (elapsed > Properties.Settings.Default.Timeout)
                {
                    //TODO: make register with deregister=true message, add to synchronization queue
                    Console.WriteLine("TIMEOUT of {0}. Deregistering.", activeComponents[who].ComponentType);
                    DataSetOps.HandleClientMalfunction(activeComponents, who, dataSets);
                    activeComponents.Remove(who);
                    return;
                }
                Thread.Sleep((int) Properties.Settings.Default.Timeout);
            }
        }

        private void RunStatusThread(int who, 
            IDictionary<int, ActiveComponent> activeComponents, IDictionary<int, ProblemDataSet> dataSets)
        {
            var t = new Thread(() => StatusThreadWork(who, activeComponents, dataSets));
            t.Start();
            _currentlyWorkingThreads.Add(t);
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
                case MessageType.SolveRequestResponseMessage:
                    return RespondSolveRequestResponseMessage(message.Cast<SolveRequestResponse>(), dataSets, activeComponents);
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
            //nothing. noOperation is not enqueued anywhere
        }

        protected virtual void ProcessSolvePartialProblemMessage(SolvePartialProblems message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            WriteControlInformation(message);
            //TODO: add to synchronization queue
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
            //TODO: add to synchronization queue
            //TODO: implement it on backup side
            WriteControlInformation(message);
            //message delivered from TM or CN
            //in case of TM - it is final solution. adjust dataset for proper problemId
            //that means, just make only one partialSet with solutions as given from Solutions message
            //in case of CN - it is partial solution. adjust partialSet array element (for taskId) 
            //in proper problemId
            if (message.SolutionsList == null || message.SolutionsList.Length == 0)
                return;

            int key = (int)message.Id;
            if (!dataSets.ContainsKey(key))
                return;
            //this is from TM:
            if (message.SolutionsList.Length == 1 && message.SolutionsList[0].Type == SolutionsSolutionType.Final)
            {
                dataSets[key].PartialSets = new PartialSet[1];
                dataSets[key].PartialSets[0] = new PartialSet()
                {
                    NodeId = 0,
                    PartialProblem = null,
                    PartialSolution = message.SolutionsList[0],
                    Status = PartialSetStatus.Sent
                };
            }
            //this is from CN
            else
            {
                //only one solution is delivered by CN at a time
                if (message.SolutionsList.Length != 1)
                    return;
                var taskId = message.SolutionsList[0].TaskId;
                foreach (var partialSet in dataSets[key].PartialSets)
                {
                    if (partialSet.PartialProblem.TaskId == taskId)
                    {
                        partialSet.PartialSolution = message.SolutionsList[0];
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
            //TODO: add to synchronization queue
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
            //nothing. msg delivered to TM. to backup too, but it only processes it
            return null;
        }

        protected virtual Message[] RespondNoOperationMessage(NoOperation message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            //nothing. noOperation is not enqueued
            return null;
        }

        protected virtual Message[] RespondSolvePartialProblemMessage(SolvePartialProblems message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents, List<BackupServerInfo> backups)
        {
            //sent by TM. send noOperation only.
            return new Message[] { new NoOperation()
            {
                BackupServersInfo = backups.ToArray()
            }
            };
        }

        protected virtual Message[] RespondRegisterMessage(Register message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents, List<BackupServerInfo> backups)
        {
            //add new entity to ActiveComponents, create immediately registerResponse message
            //with this id
            int maxId = activeComponents.Count== 0 ? 1 : activeComponents.Keys.Max() + 1;
            var newComponent = new ActiveComponent()
            {
                ComponentType = message.Type.Value,
                SolvableProblems = message.SolvableProblems
            };
            activeComponents.Add(maxId, newComponent);
            Console.WriteLine("New component: {0}, assigned id: {1}", message.Type, maxId);
            log.DebugFormat("New component: {0}, assigned id: {1}", message.Type, maxId);
            //add new watcher of timeout
            RunStatusThread(maxId, activeComponents, dataSets);
            //TODO: add register message to synchronization queue (with id specified)
            return new Message[]
            {
                new RegisterResponse()
                {
                    Id = (ulong) maxId,
                    Timeout = Properties.Settings.Default.Timeout
                    //backups is obsolete. schema has already changed
                },
                new NoOperation()
                {
                    BackupServersInfo = backups.ToArray()
                }
            };
        }

        protected virtual Message[] RespondRegisterResponseMessage(RegisterResponse message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            //nothing. same reason as RespondDivideProblemMessage
            return null;
        }

        protected virtual Message[] RespondSolutionsMessage(Solutions message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents, List<BackupServerInfo> backups)
        {
            //sent by CN or TM. send NoOperation only.
            return new Message[] { new NoOperation()
            {
                BackupServersInfo = backups.ToArray()
            }
            };
        }

        protected virtual Message[] RespondSolutionRequestMessage(SolutionRequest message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents, List<BackupServerInfo> backups)
        {
            //sent by client node. send NoOperation + CaseExtractor.GetSolutionState
            var solutionState = DataSetOps.GetSolutionState(message, dataSets);
            if (solutionState == null)
            {
                return new Message[] { new NoOperation()
                    {
                        BackupServersInfo = backups.ToArray()
                    }
                };
            }

            return new Message[] {solutionState, new NoOperation()
                    {
                        BackupServersInfo = backups.ToArray()
                    }};
        }

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
            Console.WriteLine("New problem, ProblemType={0}. Assigned id: {1}", 
                message.ProblemType, maxProblemId);
            log.DebugFormat("New problem, ProblemType={0}. Assigned id: {1}",
                message.ProblemType, maxProblemId);
            //TODO: add proper solveRequest message to synchronization queue

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

        protected virtual Message[] RespondSolveRequestResponseMessage(SolveRequestResponse message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            //nothing. delivered to client node only. nowhere else
            return null;
        }

        protected virtual Message[] RespondStatusMessage(Status message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents, List<BackupServerInfo> backups)
        {
            //TODO: implementation on backup side
            //if sent by TM - send NoOp + return from CaseExtractor.GetMessageForTaskManager
            //if sent by CN - send NoOp + return from CaseExtractor.GetMessageForCompNode
            int who = (int)message.Id;
            if (!activeComponents.ContainsKey(who))
                return new Message[] {new Error() {ErrorMessage = "who are you?",
                    ErrorType = ErrorErrorType.UnknownSender} };

            activeComponents[who].StatusWatch.Restart();

            Message whatToDo = null;
            Console.WriteLine("Handling status message of {0}(id={1}). Searching for problems.",
                activeComponents[who].ComponentType, who);
            log.DebugFormat("Handling status message of {0}(id={1}).",
                activeComponents[who].ComponentType, who);
            switch (activeComponents[who].ComponentType)
            {
                case ComponentType.ComputationalNode:
                    whatToDo = DataSetOps.GetMessageForCompNode(activeComponents, who, dataSets);
                    break;
                    case ComponentType.TaskManager:
                    whatToDo = DataSetOps.GetMessageForTaskManager(activeComponents, who, dataSets);
                    break;
                    case ComponentType.CommunicationServer:
                    //TODO: sent by backup - we don't know yet what 
                    //TODO: to send, probably whole Synchronization Queue
                    break;
            }
            if (whatToDo == null)
            {
                Console.WriteLine("Nothing additional found for {0} (id={1})", 
                    activeComponents[who].ComponentType, who);
                log.DebugFormat("Nothing additional found for {0} (id={1})",
                    activeComponents[who].ComponentType, who);
                return new Message[]
                {
                    new NoOperation()
                    {
                        BackupServersInfo = backups.ToArray()
                    }
                };
            }
            Console.WriteLine("Found problem ({0}) for {1} (id={2})", 
                whatToDo.MessageType, activeComponents[who].ComponentType, who);
            log.DebugFormat("Found problem ({0}) for {1} (id={2})",
                whatToDo.MessageType, activeComponents[who].ComponentType, who);
            //TODO: add whatToDo to synchronization queue
            return new Message[]
            {
                whatToDo, 
                new NoOperation()
                {
                    BackupServersInfo = backups.ToArray()
                }
            };
        }

        protected virtual Message[] RespondErrorMessage(Error message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            //practically nothing to do
            //warn logger, print something on console if verbose
            log.DebugFormat("Error message acquired. Type={0}, Message={1}",
                message.ErrorType, message.ErrorMessage);
            Console.WriteLine("Error message acquired. Type={0}, Message={1}",
                message.ErrorType, message.ErrorMessage);
            return null;
        }
    }
}