﻿using System;
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
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
            log.Debug("Registering CN...");
            Console.WriteLine("Registering CN...");
            RegisterComponent();
            log.Debug(string.Format("Registering complete with id={0}", componentId));
            Console.WriteLine("Registering complete with id={0}", componentId);
            core.ComponentId = this.componentId;
            while (true)
            {
                log.Debug(string.Format("Sleeping (less than timeout={0})", timeout));
                Thread.Sleep((int)(0.7 * timeout));
                log.Debug("Sending status");
                Console.WriteLine("Sending status");
                Message[] responses = this.SendStatus();
                log.Debug("Status sent");
                Console.WriteLine("Status sent");
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
                Type = new RegisterType()
                {
                    Value = ComponentType.ComputationalNode
                },
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
                        log.Debug("NoOperation acquired: updating backups");
                        Console.WriteLine("NoOperation acquired: updating backups");
                        UpdateBackups(message.Cast<NoOperation>());
                        break;
                    case MessageType.SolvePartialProblemsMessage:
                        log.Debug("SolvePartialProblems acquired: processing...");
                        Thread compThread = new Thread( o =>
                        this.StartLongComputation(() => core.ComputeSubtask
                            (message.Cast<SolvePartialProblems>())));
                        compThread.Start();
                        break;
                    case MessageType.ErrorMessage:
                        log.Debug(string.Format("Error message acquired:{0}", message.Cast<Error>().ErrorMessage));
                        break;
                    default:
                        throw new Exception("Wrong message delivered to CN: " + message.ToString());
                }
            }
        }

        public override Message[] SendStatus()
        {
            Status status = core.GetStatus();
            status.Id = this.componentId;
            Message[] requests = creator.Create(status);
            return this.SendMessages(clusterClient, requests);
        }
    }
}
