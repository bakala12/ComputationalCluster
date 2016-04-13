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
        private readonly Dictionary<ulong, ProblemInfo> _currentProblems;

        public TaskManagerStorage()
        {
            _currentProblems = new Dictionary<ulong, ProblemInfo>();
        }

        internal void AddIssue(ulong id, ProblemInfo problemInfo)
        {
            _currentProblems.Add(id, problemInfo);
        }

        internal bool ContainsIssue(ulong id)
        {
            return _currentProblems.ContainsKey(id);
        }

        internal bool ExistsTask(ulong id, ulong taskId)
        {
            return _currentProblems.ContainsKey(id) && _currentProblems[id].
                PartialSolutions.ContainsKey(taskId);
        }

        internal bool IssueCanBeLinked(ulong id)
        {
            return _currentProblems[id].ProblemsCount == 
                _currentProblems[id].SolutionsCount;
        }

        internal void AddTaskToIssue(ulong id, SolvePartialProblemsPartialProblem partialProblem)
        {
            _currentProblems[id].PartialSolutions.Add(partialProblem.TaskId, null);
        }

        internal string GetIssueType(ulong problemId)
        {
            return _currentProblems[problemId].ProblemType;
        }

        internal void AddSolutionToIssue(ulong id, ulong taskId, SolutionsSolution solution)
        {
            _currentProblems[id].PartialSolutions[taskId] = solution;
            _currentProblems[id].SolutionsCount++;
        }

        public void RemoveIssue(ulong id)
        {
            _currentProblems.Remove(id);
        }

        public byte[][] GetIssueSolutionsBytes(ulong problemId)
        {
            return
                _currentProblems[problemId].PartialSolutions.Select(solution => solution.Value.Data).ToArray();
        }

        public byte[] GetCommonData(ulong problemId)
        {
            return _currentProblems[problemId].CommonData;
        }
    }
}
