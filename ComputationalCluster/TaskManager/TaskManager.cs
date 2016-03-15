using CommunicationsUtils.ClientComponentCommon;
using CommunicationsUtils.Messages;
using CommunicationsUtils.NetworkInterfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using TaskManager.Core;

namespace TaskManager
{
    /// <summary>
    /// task manager communication context
    /// uses 2+ threads
    /// status sending thread - used in Run(), sends status via statusClient
    /// message processing thread - works in HandleResponses, checks responses queue
    /// long-running threads - invoked in message processing, work on divide/link problems
    /// and send proper messages via problemClient
    /// </summary>
    public class TaskManager : InternalClientComponent
    {
        //task manager non-communication context
        private ITaskManagerProcessing core;

        public TaskManager(IClusterClient _statusClient, IClusterClient _problemClient, 
            IMessageArrayCreator _creator, ITaskManagerProcessing _core) 
            : base (_statusClient, _problemClient, _creator)
        {
            core = _core;
        }

        public override void Run ()
        {
            //run handler thread
            Thread handlerThread = new Thread(this.HandleResponses);
            handlerThread.Start();

            //this thread becomes now status sending thread
            RegisterComponent();
            core.ComponentId = this.componentId;
            while(true)
            {
                Thread.Sleep((int)(0.7 * timeout));
                Message[] responses = this.SendStatus();
                foreach (var response in responses)
                {
                    messageQueue.Enqueue(response);
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
            Message[] requests = creator.Create(status);
            return statusClient.SendRequests(requests);
        }

        /// <summary>
        /// provides proper register message
        /// </summary>
        public override void RegisterComponent()
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
        /// handler of respones, sends proper requests
        /// </summary>
        /// <param name="responses"></param>
        public override void HandleResponses ()
        {
            while (true)
            {
                Message message;
                if (!messageQueue.TryDequeue (out message))
                {
                    Thread.Sleep(100);
                    continue;
                }
                switch (message.MessageType)
                {
                    case MessageType.NoOperationMessage:
                        UpdateBackups(message.Cast<NoOperation>());
                        break;
                    case MessageType.DivideProblemMessage:
                        //should be done in another thread not to
                        //overload message handler thread
                        Thread compThread = new Thread
                            (o=> this.StartLongComputation(() => core.DivideProblem
                        (message.Cast<DivideProblem>())));
                        compThread.Start();
                        break;
                    case MessageType.SolutionsMessage:
                        //first, in this thread, if solution needs to be linked,
                        //create new thread
                        Thread solThread = new Thread (o=> 
                        core.HandleSolutions((message.Cast<Solutions>())));
                        solThread.Start();
                        break;
                    case MessageType.ErrorMessage:
                        //something?
                        break;
                    default:
                        throw new Exception("Wrong message delivered to TM: " + message.ToString());
                }
            }
        }

        /// <summary>
        /// nothing for now:
        /// </summary>
        /// <param name="msg"></param>
        public override void UpdateBackups (NoOperation msg)
        {
            //will be implemented in internalclientcomponent in the next week
        }
    }
}
