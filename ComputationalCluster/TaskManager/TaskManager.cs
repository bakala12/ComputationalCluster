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
        private TaskManagerMessageProcessor core;

        public TaskManager(IClusterClient _statusClient, IClusterClient _problemClient, 
            IMessageArrayCreator _creator, TaskManagerMessageProcessor _core) 
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
            Console.WriteLine("Registering TM...");
            RegisterComponent();
            core.ComponentId = this.componentId;
            Console.WriteLine("Registering complete with id={0}", componentId);
            while(true)
            {
                Thread.Sleep((int)(0.7 * timeout));
                Console.WriteLine("Sending status");
                Message[] responses = this.SendStatus();
                Console.WriteLine("Status sent");
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
            return this.SendMessages(clusterClient, requests);
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
        ///
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
                        Console.WriteLine("NoOperation acquired: updating backups");
                        UpdateBackups(message.Cast<NoOperation>());
                        break;
                    case MessageType.DivideProblemMessage:
                        //should be done in another thread not to
                        //overload message handler thread
                        Console.WriteLine("DivideProblem acquired: dividing problem processing...");
                        DivideProblem msg = message.Cast<DivideProblem>();
                        Thread compThread = new Thread
                            (o=> this.StartLongComputation(() => core.DivideProblem
                        (msg)));
                        compThread.Start();
                        break;
                    case MessageType.SolutionsMessage:
                        //first, in this thread, if solution needs to be linked,
                        //create new thread
                        Console.WriteLine("Solutions acquired: solutions msg processing");
                        Solutions smsg = message.Cast<Solutions>();
                        Thread solThread = new Thread (o=> this.StartLongComputation
                        (() => core.HandleSolutions(smsg)));
                        solThread.Start();
                        break;
                    case MessageType.ErrorMessage:
                        Console.WriteLine("Error message acquired:{0}",message.Cast<Error>().ErrorMessage);
                        break;
                    default:
                        throw new Exception("Wrong message delivered to TM: " + message.ToString());
                }
            }
        }
    }
}
