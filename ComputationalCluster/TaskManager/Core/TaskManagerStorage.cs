using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicationsUtils.Messages;

namespace TaskManager.Core
{
    /// <summary>
    /// class for storing and handling problems in TM's memory
    /// </summary>
    public class TaskManagerStorage
    {
        private Dictionary<ulong, ProblemInfo> currentProblems;

        public TaskManagerStorage()
        {
            currentProblems = new Dictionary<ulong, ProblemInfo>();
        }

        internal void AddIssue(ulong id, ProblemInfo problemInfo)
        {
            currentProblems.Add(id, problemInfo);
        }

        internal bool ContainsIssue(ulong id)
        {
            return currentProblems.ContainsKey(id);
        }

        internal bool ExistsTask(ulong id, ulong taskId)
        {
            return currentProblems.ContainsKey(id) && currentProblems[id].
                PartialSolutions.ContainsKey(taskId);
        }

        internal bool IssueCanBeLinked(ulong id)
        {
            return currentProblems[id].ProblemsCount == 
                currentProblems[id].SolutionsCount;
        }

        internal void AddTaskToIssue(ulong id, SolvePartialProblemsPartialProblem partialProblem)
        {
            currentProblems[id].PartialSolutions.Add(partialProblem.TaskId, null);
        }

        internal string GetIssueType(ulong problemId)
        {
            return currentProblems[problemId].ProblemType;
        }

        internal void AddSolutionToIssue(ulong id, ulong taskId, SolutionsSolution solution)
        {
            currentProblems[id].PartialSolutions[taskId] = solution;
        }

        public void RemoveIssue(ulong id)
        {
            currentProblems.Remove(id);
        }
    }
}
