using CommunicationsUtils.ClientComponentCommon;
using CommunicationsUtils.Messages;
using CommunicationsUtils.NetworkInterfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using TaskManager.Core;

namespace TaskManager
{
    /// <summary>
    /// task manager communication context
    /// </summary>
    public class TaskManager : InternalClientComponent
    {
        //task manager non-communication context
        private ITaskManagerProcessing core;
        private Queue<Message> messageQueue;

        public TaskManager(IClusterClient _clusterClient, IClusterClient _special, 
            IMessageArrayCreator _creator, ITaskManagerProcessing _core) 
            : base (_clusterClient, _special, _creator)
        {
            core = _core;
        }

        public override void Run ()
        {
            //run handler thread
            Thread handlerThread = new Thread(this.HandleResponses);
            handlerThread.Start();

            registerComponent();
            while(true)
            {
                Message[] responses = SendStatus();
                foreach (var response in responses)
                {
                    messageQueue.Enqueue(response);
                }
                Thread.Sleep((int)(0.7 * timeout));
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
            return clusterClient.SendRequests(requests);
        }

        /// <summary>
        /// handler of respones, sends proper requests
        /// </summary>
        /// <param name="responses"></param>
        public void HandleResponses ()
        {
            while (true)
            {
                //busy waiting. must be changed.
                if (messageQueue.Count == 0)
                {
                    Thread.Sleep(50);
                    continue;
                }
                var message = messageQueue.Dequeue();

                switch (message.MessageType)
                {
                    case MessageType.NoOperationMessage:
                        UpdateBackups(message.Cast<NoOperation>());
                        break;
                    case MessageType.DivideProblemMessage:
                        //should be done in another thread not to
                        //overload message handler thread
                        Thread compThread = new Thread
                            (o=> this.longComputation(() => core.DivideProblem
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
                        //case errormsg
                    default:
                        throw new Exception("Wrong message delivered to TM: " + message.ToString());
                }
            }
        }

        private void sendSomething (Message request)
        {
            Message[] requests = creator.Create(request);
            Message[] responses = specialClusterClient.SendRequests(requests);
            foreach (var response in responses)
                messageQueue.Enqueue(response);
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

        private void longComputation (Func<Message> a)
        {
            Message m = a.Invoke();
            if (m != null)
                sendSomething(m);
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
