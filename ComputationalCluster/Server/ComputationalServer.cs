﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using CommunicationsUtils.Messages;
using CommunicationsUtils.NetworkInterfaces;
using Server.Data;
using Server.Interfaces;
using Server.MessageProcessing;

namespace Server
{
    public class ComputationalServer : IRunnable
    {
        /// <summary>
        /// Indicating whether server threads work.
        /// </summary>
        private volatile bool _isWorking;
        
        /// <summary>
        /// A list of currently running threads at server.
        /// </summary>
        private readonly List<Thread> _currentlyWorkingThreads = new List<Thread>(); 

        /// <summary>
        /// An object for multithread synchronization.
        /// </summary>
        private readonly object _syncRoot = new object();

        /// <summary>
        /// Listener which allows to receive and send messages.
        /// </summary>
        private readonly IClusterListener _clusterListener;

        /// <summary>
        /// Stores messages in queue
        /// </summary>
        private readonly ConcurrentQueue<Message> _messagesQueue;

        private List<BackupServerInfo> _backups;

        /// <summary>
        /// Current state of server.
        /// </summary>
        public ServerState State { get; private set; }

        /// <summary>
        /// List of active components in the system.
        /// INDEXED by componentId - assigned by server
        /// </summary>
        private ConcurrentDictionary<int, ActiveComponent> _activeComponents;

        /// <summary>
        /// List of active problem data sets.
        /// INDEXED by problemId - assigned by server
        /// </summary>
        private ConcurrentDictionary<int, ProblemDataSet> _problemDataSets;

        /// <summary>
        /// Object responsible for processing messages.
        /// </summary>
        private readonly IMessageProcessor _messageProcessor;
        
        /// <summary>
        /// Initializes a new instance of ComputationalServer with the specified listener.
        /// The default state of server is Backup.
        /// </summary>
        /// <param name="listener">Listener object which handle communication.</param>
        public ComputationalServer(IClusterListener listener)
        {
            Console.WriteLine("Creating new instance of ComputationalServer.");
            if (listener == null) throw new ArgumentNullException(nameof(listener));
            _clusterListener = listener;
            State = ServerState.Primary;
            _messagesQueue = new ConcurrentQueue<Message>();
            _activeComponents = new ConcurrentDictionary<int, ActiveComponent>();
            _problemDataSets= new ConcurrentDictionary<int, ProblemDataSet>();
            _messageProcessor = new PrimaryMessageProcessor();
            _backups = new List<BackupServerInfo>();
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
            if(state == ServerState.Primary)
                _messageProcessor = new PrimaryMessageProcessor();
            Console.WriteLine("New instance of ComputationalServer has been created.");
        }

        /// <summary>
        /// Starts server work.
        /// </summary>
        public void Run()
        {
            //TODO: BACKUP IMPLEMENTATION

            //TODO: if is backup, start status sending and enqueueing things on one thread,
            //TODO: and dequeueing and updating data set on another

            //TODO: if is primary, start listening and responding on one thread,
            //TODO: and updating data set on another (just like here below)

            Console.WriteLine("Starting listening mechanism.");
            _clusterListener.Start();
            _isWorking = true;
            _currentlyWorkingThreads.Clear();
            Console.WriteLine("Listening mechanism has been started.");
            DoWork();
        }

        /// <summary>
        /// Stops server work.
        /// </summary>
        public void Stop()
        {
            Console.WriteLine("Stopping threads.");
            _clusterListener.Stop();
            _isWorking = false;
            foreach (var currentlyWorkingThread in _currentlyWorkingThreads)
            {
                currentlyWorkingThread?.Join();
            }
            _currentlyWorkingThreads.Clear();
            Console.WriteLine("Threads have been stopped.");
        }

        /// <summary>
        /// Starts void argumentsless delegate in new thread.
        /// </summary>
        /// <param name="delegatFunc">Function to be invoked in a separate thread</param>
        private void ProcessInParallel(Action delegatFunc)
        {
            var thread = new Thread(()=>delegatFunc());
            _currentlyWorkingThreads.Add(thread);
            thread.Start();
        }

        /// <summary>
        /// Delegate for listening and storing messages thread.
        /// </summary>
        private void ListenAndStoreMessagesAndSendResponses()
        {
            while (_isWorking)
            {
                lock (_syncRoot)
                {
                    Console.WriteLine("Waiting for request messages.");
                    var requestsMessages = _clusterListener.WaitForRequest();
                    if(requestsMessages==null) Console.WriteLine("No request messages detected.");
                    // ReSharper disable once PossibleNullReferenceException
                    Console.WriteLine("Request messages has been awaited. Numer of request messages: " + requestsMessages.Length);
                    foreach (var message in requestsMessages)
                    {
                        //TODO: not all messages should be enqueued (no SolveRequest, no Register)
                        _messagesQueue.Enqueue(message);
                        Console.WriteLine("Enqueueing {0} message.", message.MessageType);
                        var responseMessages = _messageProcessor.CreateResponseMessages(message, _problemDataSets,
                            _activeComponents, _backups);
                        _clusterListener.SendResponse(responseMessages);
                        Console.WriteLine("Response for {0} message has been sent.", message.MessageType);
                    }
                }
            }
        }

        /// <summary>
        /// Delegate for dequeueing and processing messages thread.
        /// </summary>
        private void DequeueMessagesAndUpdateProblemStructures()
        {
            while (_isWorking)
            {
                lock (_syncRoot)
                {
                    Message message;
                    var result = _messagesQueue.TryDequeue(out message);
                    if (!result) continue;
                    Console.WriteLine("Dequeueing {0} message.", message.MessageType);
                    _messageProcessor.ProcessMessage(message, _problemDataSets, _activeComponents);
                    Console.WriteLine("Message {0} has been proccessed.", message.MessageType);
                }
            }
        }

        /// <summary>
        /// Server work function.
        /// </summary>
        protected virtual void DoWork()
        {
            Console.WriteLine("Starting new thread for listening, storing messages and sending responses.");
            ProcessInParallel(ListenAndStoreMessagesAndSendResponses);
            Console.WriteLine("Thread for listening, storing messages and sending responses has been started.");
            Console.WriteLine("Starting new thread for dequeueing messages and updating additional sets.");
            ProcessInParallel(DequeueMessagesAndUpdateProblemStructures);
            Console.WriteLine("Thread for dequeueing messages and updating additional sets has been started.");
        }
    }
}