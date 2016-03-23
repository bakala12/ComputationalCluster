using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using CommunicationsUtils.Messages;
using Server.Data;

namespace Server.MessageProcessing
{
    //TODO: implementation.
    /// <summary>
    /// Message processor for backup server.
    /// Contains implementations for handling different messages that occur in backup server.
    /// </summary>
    public class BackupMessageProcessor : MessageProcessor
    {
        public BackupMessageProcessor(ConcurrentQueue<Message> _synchronizationQueue) : 
            base (_synchronizationQueue)
        { }

        protected override Message[] RespondStatusMessage(Status message,
           IDictionary<int, ProblemDataSet> dataSets,
           IDictionary<int, ActiveComponent> activeComponents, List<BackupServerInfo> backups)
        {
            WriteResponseMessageControlInformation(message, MessageType.SolutionsMessage);
            Message[] msgs = _synchronizationQueue.ToArray();
            _synchronizationQueue = new ConcurrentQueue<Message>();
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
            if (message.Deregister)
            {
                activeComponents.Remove((int) message.Id);
            }
            else
            {
                activeComponents.Add((int) message.Id, new ActiveComponent()
                {
                    ComponentType = message.Type,
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
