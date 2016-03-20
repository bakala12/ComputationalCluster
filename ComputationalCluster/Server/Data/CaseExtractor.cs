using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicationsUtils.Messages;

namespace Server.Data
{
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
        /// <param name="components">active components list</param>
        /// <param name="componentId">the id of TM that send status</param>
        /// <param name="dataSets">current server problem memory</param>
        /// <returns>message for TM</returns>
        public static Message GetMessageForTaskManager
            (IDictionary<int, ActiveComponent> components, int componentId,
                IDictionary<int, ProblemDataSet> dataSets)
        {
            //TODO: get message for TM - implementation
            Message response = null;
            //checking divide problem posibilities
            foreach (var dataSet in dataSets)
            {
                //no division yet, and problem type is supported
                if (components[componentId].SolvableProblems.Contains(dataSet.Value.ProblemType) &&
                    dataSet.Value.TaskManagerId == 0)
                {
                    response = new DivideProblem()
                    {
                        ComputationalNodes = 1, //we'll worry about this later
                        Data = dataSet.Value.CommonData,
                        Id = (ulong) dataSet.Key,
                        NodeID = (ulong) componentId,
                        ProblemType = dataSet.Value.ProblemType
                    };
                    dataSet.Value.TaskManagerId = componentId;
                    break;
                }
            }
            //if divide problem is here, we can send it:
            if (response != null)
                return response;

            //checking linking solutions posibilites
            foreach (var dataSet in dataSets)
            {
                //check only in your own problem assignments
                if (dataSet.Value.TaskManagerId == componentId)
                {
                    //problem is not divided yet, so nothing
                    if (dataSet.Value.PartialSets == null || dataSet.Value.PartialSets.Length == 0)
                    {
                        break;
                    }
                    //check potential solutions-to-link
                    List<SolutionsSolution> solutionsToSend = new List<SolutionsSolution>();
                    foreach (var partialSet in dataSet.Value.PartialSets)
                    {
                        //there is solution from CN, can be sent
                        if (partialSet.Status == PartialSetStatus.Ongoing &&
                            partialSet.PartialSolution != null)
                        {
                            solutionsToSend.Add(partialSet.PartialSolution);
                            partialSet.Status = PartialSetStatus.Sent;
                        }
                    }
                    //if can be linked, then make a message and break
                    if (solutionsToSend.Count > 0)
                    {
                        return new Solutions()
                        {
                            CommonData = dataSet.Value.CommonData,
                            Id = (ulong) dataSet.Key,
                            ProblemType = dataSet.Value.ProblemType,
                            Solutions1 = solutionsToSend.ToArray()
                        };
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// gets some problem for comp. node (response to status msg)
        /// </summary>
        /// <param name="components">current active components memory</param>
        /// <param name="componentId">the ID of a comp node that sent status msg</param>
        /// <param name="dataSets">memory context</param>
        /// <returns>SolvePartialProblem message - null if there is nothing to do</returns>
        public static SolvePartialProblems GetMessageForCompNode
            (IDictionary<int, ActiveComponent> components, int componentId,
                IDictionary<int, ProblemDataSet> dataSets)
        {
            SolvePartialProblems response = null;

            foreach (var dataSet in dataSets)
            {
                //checking only problems that this CN can handle
                if (components[componentId].SolvableProblems.Contains(dataSet.Value.ProblemType))
                {
                    //no partial problems for this problem yet
                    if (dataSet.Value.PartialSets == null)
                        continue;
                    //check if there is some problem to send
                    foreach (var partialSet in dataSet.Value.PartialSets)
                    {
                        //problem can be sent - because its fresh
                        //we send only one partial problem to CN at a time
                        if (partialSet.Status == PartialSetStatus.Fresh)
                        {
                            response = new SolvePartialProblems()
                            {
                                CommonData = dataSet.Value.CommonData,
                                Id = (ulong) dataSet.Key,
                                PartialProblems = new[] {partialSet.PartialProblem},
                                ProblemType = dataSet.Value.ProblemType,
                                SolvingTimeoutSpecified = false //we'll worry about this later
                            };
                            partialSet.Status = PartialSetStatus.Ongoing;
                            partialSet.NodeId = componentId;
                            break;
                        }
                    }
                }
                //found a message, can jump out
                if (response != null)
                    break;
            }
            return response;
        }

        /// <summary>
        /// gets computation stage as reponse for client node
        /// </summary>
        /// <param name="request">request with proper info</param>
        /// <param name="dataSets">data sets in server's memory</param>
        /// <returns></returns>
        public static Solutions GetSolutionState
            (SolutionRequest request, IDictionary<int, ProblemDataSet> dataSets)
        {

            int key = (int) request.Id;
            //something can go very very wrong:
            if (!dataSets.ContainsKey(key))
            {
                return null;
            }

            //template of response indicating that the problem is "ongoing"
            //this is very inconsistent, but i did not write the specification
            Solutions response = new Solutions()
            {
                CommonData = dataSets[key].CommonData,
                Id = (ulong) key,
                ProblemType = dataSets[key].ProblemType,
                Solutions1 = new[]
                {
                    new SolutionsSolution()
                    {
                        ComputationsTime = 0,
                        Data = null,
                        TaskIdSpecified = false,
                        Type = SolutionsSolutionType.Ongoing
                    }
                }
            };

            if (dataSets[key].PartialSets == null || dataSets[key].PartialSets.Length == 0)
                return response;

            //response will be the status of first partial solution in problem memory (will be final
            //when TM would link it)
            if (dataSets[key].PartialSets[0].PartialSolution != null)
            {
                response = new Solutions()
                {
                    CommonData = dataSets[key].CommonData,
                    Id = (ulong) key,
                    ProblemType = dataSets[key].ProblemType,
                    Solutions1 = new[] {dataSets[key].PartialSets[0].PartialSolution}
                };
            }

            return response;
        }
    }
}
