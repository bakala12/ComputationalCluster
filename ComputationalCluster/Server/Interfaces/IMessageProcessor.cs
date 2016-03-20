using System.Collections.Generic;
using CommunicationsUtils.Messages;
using Server.Data;

namespace Server.Interfaces
{
    /// <summary>
    /// Base interface for handling and processing messages.
    /// </summary>
    public interface IMessageProcessor
    {
        /// <summary>
        /// Processes message.
        /// </summary>
        /// <param name="message">Instance of message to process</param>
        /// <param name="dataSets">Dictionary of problem data sets (maybe to update one of these or maybe not)</param>
        /// <param name="activeComponents">Dictionary of active components (maybe to update one of these or maybe not)</param>
        void ProcessMessage(Message message, IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents);

        /// <summary>
        /// Creates array of response messages for specified message.
        /// </summary>
        /// <param name="message">Instance of message to create response messages for</param>
        /// <param name="dataSets">Dictionary of problem data sets (maybe to update one of these or maybe not)</param>
        /// <param name="activeComponents">Dictionary of active components (maybe to update one of these or maybe not)</param>
        /// <param name="backups">backups' list</param>
        /// <returns></returns>
        Message[] CreateResponseMessages(Message message, IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents, List<BackupServerInfo> backups);
    }
}
