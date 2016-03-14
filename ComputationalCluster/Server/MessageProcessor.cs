using System;
using System.Collections.Generic;
using System.Linq;
using CommunicationsUtils.Messages;

namespace Server
{
    // przekazuje listę dataSetów aż tutaj by dopiero tu w processingu określać czy potrzeba wogóle jakiś dataSet aktualizować czy nie
    // może da się lepiej
    public class MessageProcessor
    {
        /// <summary>
        /// Processes specified message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="dataSets"></param>
        /// <param name="activeComponents"></param>
        public static void ProcessMessage(Message message, Dictionary<int, ProblemDataSet> dataSets, Dictionary<int, ActiveComponent> activeComponents)
        {
            //TODO
            throw new NotImplementedException();
            // find dataSet by problemId in message if it exists in message

        }

        /// <summary>
        /// Creates array of response messages for specified message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="dataSets"></param>
        /// <param name="activeComponents"></param>
        /// <returns></returns>
        public static Message[] CreateResponseMessages(Message message, Dictionary<int, ProblemDataSet> dataSets, Dictionary<int, ActiveComponent> activeComponents)
        {
            //TODO
            throw new NotImplementedException();
        }

        private static ProblemDataSet FindDataSetByProblemId(int problemId, IReadOnlyDictionary<int, ProblemDataSet> dataSets)
        {
            return dataSets.ContainsKey(problemId) ? dataSets[problemId] : null;
        }
    }
}