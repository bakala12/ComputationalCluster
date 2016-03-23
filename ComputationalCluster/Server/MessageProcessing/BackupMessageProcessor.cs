using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
        public BackupMessageProcessor(ConcurrentQueue<Message> synchronizationQueue,
            IDictionary<int, ProblemDataSet> dataSets, 
            IDictionary<int, ActiveComponent> activeComponents ) : 
            base (synchronizationQueue, dataSets, activeComponents)
        { }

        protected override Message[] RespondStatusMessage(Status message,
           IDictionary<int, ProblemDataSet> dataSets,
           IDictionary<int, ActiveComponent> activeComponents, List<BackupServerInfo> backups)
        {
            WriteResponseMessageControlInformation(message, MessageType.SolutionsMessage);
            var msgs = SynchronizationQueue.ToArray();
            SynchronizationQueue = new ConcurrentQueue<Message>();
            return msgs;
        }


        protected override void ProcessDivideProblemMessage(DivideProblem message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            WriteControlInformation(message);
            DataSetOps.HandleDivideProblem(message, dataSets);
        }

        protected override void ProcessNoOperationMessage(NoOperation message,
        IDictionary<int, ProblemDataSet> dataSets,
        IDictionary<int, ActiveComponent> activeComponents)
        {
            WriteControlInformation(message);
            // TODO: ewentualnie aktualizacja listy backupow 
        }

        protected override void ProcessRegisterMessage(Register message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            if (message.DeregisterSpecified)
            {
                activeComponents.Remove((int) message.Id);
            }
            else
            {
                activeComponents.Add((int) message.Id, new ActiveComponent()
                {
                    ComponentType = message.Type.Value,
                    SolvableProblems = message.SolvableProblems,
                    StatusWatch = new Stopwatch()
                });
            }
        }
        protected override void ProcessSolveRequestMessage(SolveRequest message,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents)
        {
            WriteControlInformation(message);
            dataSets.Add((int) message.Id, new ProblemDataSet()
            {
                CommonData = message.Data,
                PartialSets = null,
                ProblemType = message.ProblemType,
                TaskManagerId = 0
            });
        }
    }
}
