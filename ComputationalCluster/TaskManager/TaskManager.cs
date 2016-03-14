using CommunicationsUtils.ClientComponentCommon;
using CommunicationsUtils.Messages;
using CommunicationsUtils.NetworkInterfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TaskManager
{
    /// <summary>
    /// task manager communication context
    /// </summary>
    public class TaskManager : InternalClientComponent
    {
        //watch to obey timeout
        private Stopwatch timeoutWatch = new Stopwatch();
        //task manager non-communication context
        private ITaskManagerProcessing core;

        public TaskManager(IClusterClient _clusterClient, ITaskManagerProcessing _core) 
            : base (_clusterClient)
        {
            core = _core;
        }

        public override void Run ()
        {
            registerComponent();
            timeoutWatch.Start();
            while(true)
            {
                //could be adjusted:
                if (timeoutWatch.ElapsedMilliseconds > (long)(0.7*timeout))
                {
                    Message[] responses = SendStatus();
                    timeoutWatch.Restart();
                    HandleResponses(responses);
                }
            }
        }

        /// <summary>
        /// sends status, basing on core's state
        /// </summary>
        /// <returns>responses (potential tasks) from server</returns>
        public override Message[] SendStatus()
        {
            Status status = core.GetStatus();
            status.Id = this.componentId;
            return clusterClient.SendRequests(new[] { status });
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
                switch(response.MessageType)
                {
                    case MessageType.NoOperationMessage:
                        UpdateBackups(response.Cast<NoOperation>());
                        break;
                    case MessageType.DivideProblemMessage:
                        SolvePartialProblems partialProblemsMsg = 
                            core.DivideProblem(response.Cast<DivideProblem>());
                        newRequests.Add(partialProblemsMsg);

                        break;
                    case MessageType.SolutionsMessage:
                        Solutions solutions = core.HandleSolutions((response.Cast<Solutions>()));
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

        /// <summary>
        /// provides proper register message
        /// </summary>
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

        /// <summary>
        /// nothing for now:
        /// </summary>
        /// <param name="msg"></param>
        public override void UpdateBackups (NoOperation msg)
        {

        }
    }
}
