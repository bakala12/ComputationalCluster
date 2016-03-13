using CommunicationsUtils.ClientComponentCommon;
using CommunicationsUtils.Messages;
using CommunicationsUtils.NetworkInterfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TaskManager
{
    public class TaskManager : ClientComponent
    {
        private ulong threadCount = 0;
        private List<StatusThread> threads;
        Stopwatch timeoutWatch = new Stopwatch();

        /// <summary>
        /// current problems in TM indexed by problem id in cluster (given by CS)
        /// </summary>
        private Dictionary<ulong, ProblemInfo> currentProblems = new Dictionary<ulong, ProblemInfo>();

        public TaskManager(IClusterClient _clusterClient) : base (_clusterClient)
        {
            threads = new List<StatusThread>();
            //enough for this stage:
            threads.Add(new StatusThread()
            {
                HowLongSpecified = false,
                ProblemInstanceIdSpecified = false,
                State = StatusThreadState.Idle,
                ProblemType = "",
                TaskIdSpecified = true,
                TaskId = ++threadCount
            });
        }

        public override void Run ()
        {
            registerComponent();
            timeoutWatch.Start();
            while(true)
            {
                if (timeoutWatch.ElapsedMilliseconds > (long)(0.8*timeout))
                {
                    Message[] responses = sendStatus();
                    timeoutWatch.Restart();
                    HandleResponses(responses);
                }
            }
        }

        private Message[] sendStatus()
        {
            Status statusMsg = new Status()
            {
                Id = componentId,
                Threads = threads.ToArray()
            };
            return clusterClient.SendRequests(new[] { statusMsg });
        }

        /// <summary>
        /// handler of respones, sends proper requests
        /// </summary>
        /// <param name="responses"></param>
        public void HandleResponses (Message[] responses)
        {
            List<Message> newRequests = new List<Message>();
            foreach (var response in responses)
            {
                switch(response.Type)
                {
                    case MessageType.NoOperationMessage:
                        updateBackups(response.Cast<NoOperation>());
                        break;
                    case MessageType.DivideProblemMessage:
                        SolvePartialProblems partialProblemsMsg = 
                            this.HandleProblem(response.Cast<DivideProblem>());
                        newRequests.Add(partialProblemsMsg);

                        break;
                    case MessageType.SolutionsMessage:
                        Solutions solutions = this.HandleSolutions(response.Cast<Solutions>());
                        //null if linking solutions didn't occur
                        if (solutions != null)
                        {
                            newRequests.Add(solutions);
                        }

                        break;
                    default:
                        throw new Exception("Wrong message delivered to TM: " + response.ToString());
                }
            }
            Message[] newResponses = clusterClient.SendRequests(newRequests.ToArray());
            timeoutWatch.Reset();
            this.HandleResponses(newResponses);
        }

        public SolvePartialProblems HandleProblem (DivideProblem divideProblem)
        {
            //implementation in second stage, this is mocked:

            var partialProblem = new SolvePartialProblemsPartialProblem()
            {
                TaskId = 0,
                Data = new byte[] { 0 },
                NodeID = componentId
            };

            //adding info about partial problems, their task ids, and partialProblem
            //some things can be temporary (partialProblems?)
            currentProblems.Add(divideProblem.Id, new ProblemInfo() { ProblemsCount = 1,
                ProblemType = divideProblem.ProblemType, SolutionsCount = 0 });

            currentProblems[divideProblem.Id].PartialProblems.Add(0, new PartialInfo()
            { Solution = null, Problem = partialProblem });
            //end of implementation

            //creating msg
            SolvePartialProblems partialProblems = new SolvePartialProblems()
            {
                ProblemType = divideProblem.ProblemType,
                Id = divideProblem.Id,
                CommonData = divideProblem.Data,
                PartialProblems = new SolvePartialProblemsPartialProblem[]
                {
                    partialProblem
                }
            };

            return partialProblems;
        }

        /// <summary>
        /// handles solutions msg. according to specifiaction, Solutions message
        /// concerns only one problem
        /// </summary>
        /// <param name="solutions"></param>
        public Solutions HandleSolutions(Solutions solutions)
        {
            foreach (var solution in solutions.Solutions1)
            {
                if (currentProblems[solutions.Id].PartialProblems[solution.TaskId].Solution != null)
                {
                    throw new Exception("Problem with multiple sending of partialProblem by CS");
                }
                currentProblems[solutions.Id].PartialProblems[solution.TaskId].Solution = solution;
                currentProblems[solutions.Id].SolutionsCount++;
            }
            //this is not possible:
            //if (currentProblems[solutions.Id].SolutionsCount > currentProblems[solutions.Id].ProblemsCount)

            //can be linked, because all of partial problems were solved & delivered
            if (currentProblems[solutions.Id].SolutionsCount == currentProblems[solutions.Id].ProblemsCount)
            {
                Solutions finalSolution = linkSolutions(solutions.Id);
                return finalSolution;
            }

            return null;
        }

        //task solver stuff
        private Solutions linkSolutions (ulong problemId)
        {
            //foreach (var pInfo in currentProblems[problemId].PartialProblems)
            //get SolutionsSolution from pInfo and do something amazing

            //return final solution (this one is mocked)
            return new Solutions()
            {
                CommonData = new byte[] { 0 },
                Id = problemId,
                ProblemType = currentProblems[problemId].ProblemType,
                Solutions1
            = new[] { new SolutionsSolution() { Data = null, ComputationsTime = 1, TaskIdSpecified = false,
            Type = SolutionsSolutionType.Final} }
            };
        }

        protected override void registerComponent()
        {
            // some mock:
            Register registerRequest = new Register()
            {
                ParallelThreads = 1,
                SolvableProblems = new[] { "DVRP" },
                Type = RegisterType.TaskManager,
                DeregisterSpecified = false,
                IdSpecified = false
            };

            base.handleRegisterResponses(registerRequest);
        }

        public override void updateBackups (NoOperation msg)
        {

        }
    }
}
