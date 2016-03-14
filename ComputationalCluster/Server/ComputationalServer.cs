﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunicationsUtils.Messages;
using CommunicationsUtils.NetworkInterfaces;
// ReSharper disable FunctionNeverReturns

namespace Server
{
    public class ComputationalServer : IRunnable
    {
        /// <summary>
        /// Listener which allows to receive and send messages.
        /// </summary>
        private readonly IClusterListener _clusterListener;

        /// <summary>
        /// Stores messages in queue
        /// </summary>
        private readonly ConcurrentQueue<Message> _messagesQueue;

        /// <summary>
        /// Current state of server.
        /// </summary>
        public ServerState State { get; set; }

        /// <summary>
        /// List of active components in the system.
        /// </summary>
        private readonly ConcurrentDictionary<int, ActiveComponent> _activeComponents;

        /// <summary>
        /// List of active problem data sets.
        /// </summary>
        private readonly ConcurrentDictionary<int, ProblemDataSet> _problemDataSets; 

        /// <summary>
        /// Initializes a new instance of ComputationalServer with the specified listener.
        /// The default state of server is Backup.
        /// </summary>
        /// <param name="listener">Listener object which handle communication.</param>
        public ComputationalServer(IClusterListener listener)
        {
            if (listener == null) throw new ArgumentNullException(nameof(listener));
            _clusterListener = listener;
            State = ServerState.Backup;
            _messagesQueue = new ConcurrentQueue<Message>();
            _activeComponents = new ConcurrentDictionary<int, ActiveComponent>();
            _problemDataSets= new ConcurrentDictionary<int, ProblemDataSet>();
        }

        /// <summary>
        /// Initializes a new instance of ComputationalServer class withe the specified listener and 
        /// a speciefied server state.
        /// </summary>
        /// <param name="listener">Listener object which handle communication.</param>
        /// <param name="state">Server startup state.</param>
        public ComputationalServer(IClusterListener listener, ServerState state) : this(listener)
        {
            State = state;
        }

        /// <summary>
        /// Starts server work.
        /// </summary>
        public void Run()
        {
            _clusterListener.Start();
        }

        /// <summary>
        /// Stops server work.
        /// </summary>
        public void Stop()
        {
            _clusterListener.Stop();
        }

        /// <summary>
        /// Starts void argumentsless delegate in new thread.
        /// </summary>
        /// <param name="delegatFunc"></param>
        private static void ProcessInParallel(Action delegatFunc)
        {
            Task.Run(delegatFunc);
        }

        /// <summary>
        /// Delegate for listening and storing messages thread.
        /// </summary>
        private void ListenAndStoreMessagesAndSendResponses()
        {
            while (true)
            {
                var requestsMessages = _clusterListener.WaitForRequest();
                foreach (var message in requestsMessages)
                {
                    _messagesQueue.Enqueue(message);
                    var responseMessages = MessageProcessor.CreateResponseMessages(message, _problemDataSets, _activeComponents);
                    _clusterListener.SendResponse(responseMessages);
                }
            }
        }

        /// <summary>
        /// Delegate for dequeueing and processing messages thread.
        /// </summary>
        private void DequeueMessagesAndUpdateProblemStructures()
        {
            while (true)
            {
                Message message;
                var result = _messagesQueue.TryDequeue(out message);
                if (!result) continue;
                MessageProcessor.ProcessMessage(message, _problemDataSets, _activeComponents);
            }
        }

        /// <summary>
        /// Server work function.
        /// </summary>
        protected virtual void DoWork()
        {
            ProcessInParallel(ListenAndStoreMessagesAndSendResponses);
            ProcessInParallel(DequeueMessagesAndUpdateProblemStructures);
        }
    }
}