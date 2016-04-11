using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using CommunicationsUtils.Messages;
using CommunicationsUtils.NetworkInterfaces;
using Server.Data;

namespace Server.MessageProcessing
{
    /// <summary>
    /// Message processor for backup server.
    /// Contains implementations for handling different messages that occur in backup server.
    /// </summary>
    public class BackupMessageProcessor : MessageProcessor
    {
        public BackupMessageProcessor(IClusterListener clusterListener,
            ConcurrentQueue<Message> synchronizationQueue,
            IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents) :
            base(clusterListener, synchronizationQueue, dataSets, activeComponents)
        { }

        protected override Message[] RespondStatusMessage(Status message,
           IDictionary<int, ProblemDataSet> dataSets,
           IDictionary<int, ActiveComponent> activeComponents, List<BackupServerInfo> backups)
        {
            WriteResponseMessageControlInformation(message, MessageType.StatusMessage);
            var msgs = SynchronizationQueue.ToList();
            Log.Debug("Responding status message as backup. Sending sync queue and backups list");
            //send nooperation with no information about this backup (server imitation)
            msgs.Add(new NoOperation()
            {
                BackupServersInfo = backups.ToArray()
            });
            SynchronizationQueue.Clear();
            return msgs.ToArray();
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
                Log.Debug("Deregister acquired. Deregistering...");
                activeComponents.Remove((int)message.Id);
            }
            else
            {
                Log.Debug("Register acquired. Registering...");
                activeComponents.Add((int)message.Id, new ActiveComponent()
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
            dataSets.Add((int)message.Id, new ProblemDataSet()
            {
                CommonData = message.Data,
                PartialSets = null,
                ProblemType = message.ProblemType,
                TaskManagerId = 0
            });
        }
    }

    public static class ConcurentQueueExtender
    {
        public static void Clear<T>(this ConcurrentQueue<T> queue)
        {
            while (!queue.IsEmpty)
            {
                T obj;
                queue.TryDequeue(out obj);
            }
        }
    }
}
