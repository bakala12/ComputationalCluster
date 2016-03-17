using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommunicationsUtils.ClientComponentCommon;
using CommunicationsUtils.Messages;
using CommunicationsUtils.NetworkInterfaces;
using ComputationalNode.Core;

namespace ComputationalNode
{
    public class ComputationalNode : InternalClientComponent
    {
        private ComputationalNodeMessageProcessor core;

        public ComputationalNode(IClusterClient _statusClient, IClusterClient _problemClient,
            IMessageArrayCreator _creator, ComputationalNodeMessageProcessor _core) 
            : base(_statusClient, _problemClient, _creator)
        {
            core = _core;
        }

        public override void Run()
        {
            //run handler thread
            Thread handlerThread = new Thread(this.HandleResponses);
            handlerThread.Start();

            //this thread becomes now status sending thread
            Console.WriteLine("Registering CN...");
            RegisterComponent();
            Console.WriteLine("Registering complete with id={0}", componentId);
            core.ComponentId = this.componentId;
            while (true)
            {
                Console.WriteLine("Sleeping (less than timeout={0})", timeout);
                Thread.Sleep((int)(0.7 * timeout));
                Console.WriteLine("Sending status");
                Message[] responses = this.SendStatus();
                Console.WriteLine("Status sent.");
                foreach (var response in responses)
                {
                    messageQueue.Enqueue(response);
                }
            }
        }

        public override void RegisterComponent()
        {
            // some mock:
            Register registerRequest = new Register()
            {
                ParallelThreads = 1,
                SolvableProblems = new[] { "DVRP" },
                Type = RegisterType.ComputationalNode,
                DeregisterSpecified = false,
                IdSpecified = false
            };
            base.handleRegisterResponses(registerRequest);
        }

        /// <summary>
        /// handler of respones, sends proper requests
        /// </summary>
        /// <param name="responses"></param>
        public override void HandleResponses()
        {
            while (true)
            {
                Message message;
                if (!messageQueue.TryDequeue(out message))
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
                    case MessageType.SolvePartialProblemsMessage:
                        Console.WriteLine("SolvePartialProblems acquired: processing...");
                        Thread compThread = new Thread( o =>
                        this.StartLongComputation(() => core.ComputeSubtask
                            (message.Cast<SolvePartialProblems>())));
                        compThread.Start();
                        break;
                    case MessageType.ErrorMessage:
                        Console.WriteLine("Error message acquired:{0}", message.Cast<Error>().ErrorMessage);
                        break;
                    default:
                        throw new Exception("Wrong message delivered to TM: " + message.ToString());
                }
            }
        }

        public override Message[] SendStatus()
        {
            Status status = core.GetStatus();
            status.Id = this.componentId;
            Message[] requests = creator.Create(status);
            return statusClient.SendRequests(requests);
        }

        public override void UpdateBackups(NoOperation msg)
        {
            //will be implemented in internalclientcomponent in the next week
        }
    }
}
