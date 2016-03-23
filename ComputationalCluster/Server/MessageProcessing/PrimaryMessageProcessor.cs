using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CommunicationsUtils.Messages;
using Server.Data;

namespace Server.MessageProcessing
{
    /// <summary>
    /// Message processor for primary server.
    /// Contains implementations for handling different messages that occur in primary server.
    /// </summary>
    public class PrimaryMessageProcessor : MessageProcessor
    {

        public PrimaryMessageProcessor(ConcurrentQueue<Message> synchronizationQueue) :
            base(synchronizationQueue)
        { }
        protected override Message[] RespondRegisterResponseMessage(RegisterResponse message,
              IDictionary<int, ProblemDataSet> dataSets,
              IDictionary<int, ActiveComponent> activeComponents)
        {
            //nothing. same reason as RespondDivideProblemMessage
            return null;
        }

        protected override Message[] RespondRegisterMessage(Register message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents, List<BackupServerInfo> backups)
        {
            //add new entity to ActiveComponents, create immediately registerResponse message
            //with this id
            var maxId = activeComponents.Count == 0 ? 1 : activeComponents.Keys.Max() + 1;
            var newComponent = new ActiveComponent()
            {
                ComponentType = message.Type.Value,
                SolvableProblems = message.SolvableProblems
            };
            activeComponents.Add(maxId, newComponent);
            Log.DebugFormat("New component: {0}, assigned id: {1}", message.Type, maxId);
            Log.DebugFormat("New component: {0}, assigned id: {1}", message.Type, maxId);
            //add new watcher of timeout
            RunStatusThread(maxId, activeComponents, dataSets);
            //add register message to synchronization queue
            message.Id = (ulong)maxId;
            message.IdSpecified = true;
            SynchronizationQueue.Enqueue(message);
            if(message.Type.Value==ComponentType.CommunicationServer)
                AddBackupAddressToBackupList(backups);
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

        private void AddBackupAddressToBackupList(ICollection<BackupServerInfo> backups)
        {
           backups.Add(new BackupServerInfo()
           {
               address = "192.168.0.33",
               port = 8086
           });
        }

        protected override Message[] RespondNoOperationMessage(NoOperation message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            //nothing. noOperation is not enqueued
            return null;
        }

        protected override Message[] RespondSolvePartialProblemMessage(SolvePartialProblems message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents, List<BackupServerInfo> backups)
        {
            SynchronizationQueue.Enqueue(message);
            //sent by TM. send noOperation only.
            return new Message[] { new NoOperation()
                {
                    BackupServersInfo = backups.ToArray()
                }
            };
        }

        protected override Message[] RespondSolutionsMessage(Solutions message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents, List<BackupServerInfo> backups)
        {
            SynchronizationQueue.Enqueue(message);
            //sent by CN or TM. send NoOperation only.
            return new Message[] { new NoOperation()
            {
                BackupServersInfo = backups.ToArray()
            }
            };
        }

        protected override Message[] RespondStatusMessage(Status message,
           IDictionary<int, ProblemDataSet> dataSets,
           IDictionary<int, ActiveComponent> activeComponents, List<BackupServerInfo> backups)
        {
            //if sent by TM - send NoOp + return from CaseExtractor.GetMessageForTaskManager
            //if sent by CN - send NoOp + return from CaseExtractor.GetMessageForCompNode
            var who = (int)message.Id;
            if (!activeComponents.ContainsKey(who))
                return new Message[] {new Error() {ErrorMessage = "who are you?",
                    ErrorType = ErrorErrorType.UnknownSender} };

            activeComponents[who].StatusWatch.Restart();

            Message whatToDo = null;
            Log.DebugFormat("Handling status message of {0}(id={1}). Searching for problems.",
                activeComponents[who].ComponentType, who);
            Log.DebugFormat("Handling status message of {0}(id={1}).",
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
                    var msgs = SynchronizationQueue.ToArray();
                    SynchronizationQueue = new ConcurrentQueue<Message>();
                    return msgs;
            }
            if (whatToDo == null)
            {
                Log.DebugFormat("Nothing additional found for {0} (id={1})",
                    activeComponents[who].ComponentType, who);
                Log.DebugFormat("Nothing additional found for {0} (id={1})",
                    activeComponents[who].ComponentType, who);
                return new Message[]
                {
                    new NoOperation()
                    {
                        BackupServersInfo = backups.ToArray()
                    }
                };
            }
            Log.DebugFormat("Found problem ({0}) for {1} (id={2})",
                whatToDo.MessageType, activeComponents[who].ComponentType, who);
            Log.DebugFormat("Found problem ({0}) for {1} (id={2})",
                whatToDo.MessageType, activeComponents[who].ComponentType, who);

            SynchronizationQueue.Enqueue(whatToDo);
            return new[]
            {
                whatToDo,
                new NoOperation()
                {
                    BackupServersInfo = backups.ToArray()
                }
            };
        }

        protected override Message[] RespondSolutionRequestMessage(SolutionRequest message,
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
    }
}
