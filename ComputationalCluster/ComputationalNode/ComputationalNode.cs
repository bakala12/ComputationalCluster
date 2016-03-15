using System;
using System.Collections.Generic;
using System.Linq;
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
        private IComputationalNodeProcessing core;
        public ComputationalNode(IClusterClient _statusClient, IClusterClient _problemClient,
            IMessageArrayCreator _creator, IComputationalNodeProcessing _core) 
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
            RegisterComponent();
            core.ComponentId = this.componentId;
            while (true)
            {
                Thread.Sleep((int)(0.7 * timeout));
                Message[] responses = this.SendStatus();
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
                        UpdateBackups(message.Cast<NoOperation>());
                        break;
                    case MessageType.SolvePartialProblemsMessage:
                        Thread compThread = new Thread( o =>
                        this.StartLongComputation(() => core.ComputeSubtask
                            (message.Cast<SolvePartialProblems>())));
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
