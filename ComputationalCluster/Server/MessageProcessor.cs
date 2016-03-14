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
        /// /// <param name="dataSets"></param>
        public static void ProcessMessage(Message message, List<ProblemDataSet> dataSets)
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
        /// <returns></returns>
        public static Message[] CreateResponseMessages(Message message, List<ProblemDataSet> dataSets)
        {
            //TODO
            throw new NotImplementedException();
        }

        private static ProblemDataSet FindDataSetByProblemId(int problemId, IEnumerable<ProblemDataSet> dataSets)
        {
            return dataSets.FirstOrDefault(dataSet => dataSet.ProblemID == problemId);
        }
    }
}