using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicationsUtils.Messages;

namespace Server.Data
{
    //TODO: implementation.
    //TODO: using static methods is not very elegant.
    //TODO: consider wrapping these methods + dataSets object to some overclass
    /// <summary>
    /// simple static class for extracting proper messages from DataSet
    /// </summary>
    public static class CaseExtractor
    {
        /// <summary>
        /// gets proper problem to solve as response for status msg request from TM
        /// </summary>
        /// <param name="component">info about TM - SolvableProblems</param>
        /// <param name="dataSets"></param>
        /// <returns></returns>
        public static Message GetMessageForTaskManager
            (ActiveComponent component, ConcurrentDictionary<int, ProblemDataSet> dataSets)
        {
            //TODO: get message for TM - implementation
            //could be Solutions or DivideProblem
            return null;
        }

        /// <summary>
        /// gets some problem for comp. node (response to status msg)
        /// </summary>
        /// <param name="component">info about component - contains SolvableProblems</param>
        /// <param name="dataSets"></param>
        /// <returns></returns>
        public static SolvePartialProblems GetMessageForCompNode
            (ActiveComponent component, ConcurrentDictionary<int, ProblemDataSet> dataSets)
        {
            //TODO: get message for CN - implementation
            //could be only SolvePartialProblems
            return null;
        }

        /// <summary>
        /// gets computation stage as reponse for client node
        /// </summary>
        /// <param name="request">request with proper info</param>
        /// <param name="dataSets">data sets in server's memory</param>
        /// <returns></returns>
        public static Solutions GetSolutionState
            (SolutionRequest request, ConcurrentDictionary<int, ProblemDataSet> dataSets)
        {
            //TODO: for proper problemId, get Solutions message from dataSet
            return null;
        }
    }
}
