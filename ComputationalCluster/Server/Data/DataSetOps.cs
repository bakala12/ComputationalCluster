using System.Collections.Generic;
using System.Linq;
using CommunicationsUtils.Messages;

namespace Server.Data
{
    //TODO: using static methods is not very elegant.
    //TODO: consider wrapping these methods + dataSets object to some overclass
    /// <summary>
    /// simple static class for extracting proper messages from DataSet
    /// </summary>
    public static class DataSetOps
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
            foreach (var dataSet in dataSets.Where(dataSet => components[componentId].SolvableProblems.Contains(dataSet.Value.ProblemType) && dataSet.Value.TaskManagerId == 0))
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
            //if divide problem is here, we can send it:
            if (response != null)
                return response;

            //checking linking solutions posibilites
            foreach (var dataSet in dataSets)
            {
                //check only in your own problem assignments
                if (dataSet.Value.TaskManagerId != componentId) continue;
                //problem is not divided yet, so nothing
                if (dataSet.Value.PartialSets == null || dataSet.Value.PartialSets.Length == 0)
                {
                    break;
                }
                //check potential solutions-to-link
                var solutionsToSend = new List<SolutionsSolution>();
                foreach (var partialSet in dataSet.Value.PartialSets.Where(partialSet => partialSet.Status == PartialSetStatus.Ongoing &&
                                                                                         partialSet.PartialSolution != null))
                {
                    solutionsToSend.Add(partialSet.PartialSolution);
                    partialSet.Status = PartialSetStatus.Sent;
                }
                //if can be linked, then make a message and break
                if (solutionsToSend.Count > 0)
                {
                    return new Solutions()
                    {
                        CommonData = dataSet.Value.CommonData,
                        Id = (ulong) dataSet.Key,
                        ProblemType = dataSet.Value.ProblemType,
                        SolutionsList = solutionsToSend.ToArray()
                    };
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

            foreach (var dataSet in dataSets)
            {
                //checking only problems that this CN can handle
                if (!components[componentId].SolvableProblems.Contains(dataSet.Value.ProblemType)) continue;
                //no partial problems for this problem yet (not divided yet)
                if (dataSet.Value.PartialSets == null)
                    continue;
                //check if there is some problem to send
                foreach (var partialSet in dataSet.Value.PartialSets)
                {
                    //problem can be sent - because its fresh
                    //we send only one partial problem to CN at a time
                    if (partialSet.Status != PartialSetStatus.Fresh) continue;
                    var response = new SolvePartialProblems()
                    {
                        CommonData = dataSet.Value.CommonData,
                        Id = (ulong) dataSet.Key,
                        PartialProblems = new[] {partialSet.PartialProblem},
                        ProblemType = dataSet.Value.ProblemType,
                        SolvingTimeoutSpecified = false //we'll worry about this later
                    };
                    partialSet.Status = PartialSetStatus.Ongoing;
                    partialSet.NodeId = componentId;
                    return response;
                }
            }
            return null;
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

            var key = (int) request.Id;
            //something can go very very wrong:
            if (!dataSets.ContainsKey(key))
            {
                return null;
            }

            //template of response indicating that the problem is "ongoing"
            //this is very inconsistent, but i did not write the specification
            var response = new Solutions()
            {
                CommonData = dataSets[key].CommonData,
                Id = (ulong) key,
                ProblemType = dataSets[key].ProblemType,
                SolutionsList = new[]
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

            if (dataSets[key].PartialSets == null || dataSets[key].PartialSets.Length == 0
                || dataSets[key].PartialSets[0].PartialSolution == null)
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
                    SolutionsList = new[] {dataSets[key].PartialSets[0].PartialSolution}
                };
            }

            return response;
        }


        public static void HandleClientMalfunction (IDictionary<int, ActiveComponent> components, 
            int componentId, IDictionary<int, ProblemDataSet> dataSets)
        {
            switch (components[componentId].ComponentType)
            {
                case ComponentType.CommunicationServer:
                    //TODO: backup has broken. do something
                    break;
                case ComponentType.ComputationalNode:
                    ResetProblems(componentId, components[componentId], dataSets);
                    break;
                case ComponentType.TaskManager:
                    ResetDataSet(componentId, dataSets);
                    break;
            }

        }

        /// <summary>
        /// removes task manager id mark from problems in data sets
        /// linear complexity
        /// </summary>
        /// <param name="taskManagerId"></param>
        /// <param name="dataSets"></param>
        private static void ResetDataSet(int taskManagerId, IDictionary<int, ProblemDataSet> dataSets)
        {
            foreach (var dataSet in dataSets.Where(dataSet => dataSet.Value.TaskManagerId == taskManagerId))
            {
                dataSet.Value.TaskManagerId = 0;
                dataSet.Value.PartialSets = null;
            }
        }

        /// <summary>
        /// reset all partial problems that this comp. node is computing
        /// TODO: square complexity. should be boosted.
        /// </summary>
        /// <param name="compNodeId"></param>
        /// <param name="compNode"></param>
        /// <param name="dataSets"></param>
        private static void ResetProblems(int compNodeId, ActiveComponent compNode, 
            IDictionary<int, ProblemDataSet> dataSets)
        {
            //search for all problems that this compNode is computing
            foreach (var partialSet in from dataSet in dataSets where compNode.SolvableProblems.Contains(dataSet.Value.ProblemType) where dataSet.Value.PartialSets != null from partialSet in dataSet.Value.PartialSets.Where(partialSet => partialSet.NodeId == compNodeId) select partialSet)
            {
                partialSet.NodeId = 0;
                partialSet.Status = PartialSetStatus.Fresh;
                partialSet.PartialSolution = null;
            }
        }

        public static void HandleDivideProblem(DivideProblem message, IDictionary<int, ProblemDataSet> dataSets)
        {
            dataSets[(int) message.Id].TaskManagerId = (int) message.NodeID;
        }
    }
}
