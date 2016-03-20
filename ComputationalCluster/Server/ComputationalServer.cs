using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using CommunicationsUtils.Messages;
using CommunicationsUtils.NetworkInterfaces;
using CommunicationsUtils.NetworkInterfaces.Factories;
using Server.Data;
using Server.Interfaces;
using Server.MessageProcessing;

namespace Server
{
    public class ComputationalServer : IRunnable, IChangeServerState
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
        private IClusterListener _clusterListener;

        /// <summary>
        /// A client for backup server requests.
        /// </summary>
        private IClusterClient _backupClient;

        /// <summary>
        /// Stores messages in queue
        /// </summary>
        private readonly ConcurrentQueue<Message> _messagesQueue;

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
        private IMessageProcessor _messageProcessor;

        /// <summary>
        /// Specifies the time interval between two Status messages.
        /// </summary>
        public long BackupServerStatusInterval { get; protected set; }

        /// <summary>
        /// Private constructor that initializes server subcomponent correctly.
        /// </summary>
        /// <param name="state">Deterimenes the starting state of computational server.</param>
        private ComputationalServer(ServerState state)
        {
            State = state;
            _messagesQueue = new ConcurrentQueue<Message>();
            _activeComponents = new ConcurrentDictionary<int, ActiveComponent>();
            _problemDataSets = new ConcurrentDictionary<int, ProblemDataSet>();
            _messageProcessor = (state == ServerState.Primary)
                ? new PrimaryMessageProcessor() as IMessageProcessor
                : new BackupMessageProcessor();
        }

        /// <summary>
        /// Initializes a new instance of ComputationalServer with the specified listener.
        /// The default state of server is Backup.
        /// </summary>
        /// <param name="listener">Listener object which handle communication.</param>
        public ComputationalServer(IClusterListener listener) : this(ServerState.Primary)
        {
            if (listener == null) throw new ArgumentNullException(nameof(listener));
            _clusterListener = listener;
            Console.WriteLine("Creating new instance of ComputationalServer (primary).");
        }

        /// <summary>
        /// Initializes a new instance of ComputationalServer class withe the specified listener and 
        /// a speciefied server state.
        /// </summary>
        /// <param name="backupClient"> A client used as BS request sender.</param>
        public ComputationalServer(IClusterClient backupClient) :this(ServerState.Backup)
        {
            if(backupClient == null) throw new ArgumentNullException(nameof(backupClient));
            _backupClient = backupClient;
            Console.WriteLine("New instance of ComputationalServer (backup) has been created.");
        }

        /// <summary>
        /// Starts server work.
        /// </summary>
        public void Run()
        {
            //sample implemetnation
            if (State == ServerState.Backup)
                RunAsBackup();
            else
                RunAsPrimary();
        }

        /// <summary>
        /// Runs server as Primary server.
        /// </summary>
        public virtual void RunAsPrimary()
        {
            //TODO: Primary initialize
            lock (_syncRoot)
            {
                _messageProcessor = new PrimaryMessageProcessor();
            }
            _backupClient = null;
            if (_clusterListener == null)
                _clusterListener = ClusterListenerFactory.Factory.Create(IPAddress.Any, Properties.Settings.Default.Port);
            //TODO: Maybe reset data sets and list of active components here

            //TODO: Primary run !!!
            Console.WriteLine("Starting listening mechanism.");
            _clusterListener.Start();
            _isWorking = true;
            _currentlyWorkingThreads.Clear();
            Console.WriteLine("Listening mechanism has been started.");
            DoPrimaryWork();
        }

        /// <summary>
        /// Runs server as Backup server.
        /// </summary>
        public virtual void RunAsBackup()
        {
            //TODO: Backup initilize here
            lock(_syncRoot)
            {
                _messageProcessor = new BackupMessageProcessor();
            }
            _clusterListener = null;
            if (_backupClient == null)
                _backupClient = ClusterClientFactory.Factory.Create(Properties.Settings.Default.MasterAddress,
                    Properties.Settings.Default.MasterPort);
            //TODO: Maybe reset data sets and list of active components here.

            //TODO: Backup run
            _currentlyWorkingThreads.Clear();
            _isWorking = true;
            DoBackupWork();
        }

        /// <summary>
        /// Changes server state for the given one
        /// </summary>
        /// <param name="state">New server state.</param>
        public void ChangeState(ServerState state)
        {
            Stop();
            State = state;
            Run();
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
            _clusterListener = null;
            _backupClient = null;
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
                            _activeComponents);
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
        protected virtual void DoPrimaryWork()
        {
            Console.WriteLine("Starting new thread for listening, storing messages and sending responses.");
            ProcessInParallel(ListenAndStoreMessagesAndSendResponses);
            Console.WriteLine("Thread for listening, storing messages and sending responses has been started.");
            Console.WriteLine("Starting new thread for dequeueing messages and updating additional sets.");
            ProcessInParallel(DequeueMessagesAndUpdateProblemStructures);
            Console.WriteLine("Thread for dequeueing messages and updating additional sets has been started.");
        }

        /// <summary>
        /// Backup server function work.
        /// </summary>
        protected virtual void DoBackupWork()
        {
            //TODO: Do this things
            Console.WriteLine("Starting server as backup");
            Console.WriteLine("Starting backup client");
            Console.WriteLine("Registering backup server");
            RegisterBackupServer();
            Console.WriteLine("Backup registered successfully");
            Console.WriteLine("Starting status thread");
            ProcessInParallel((SendBackupStatusMessages));
            Console.WriteLine("Starting updating backup thread");
            ProcessInParallel(UpdateBackupServerState);
        }

        /// <summary>
        /// Updates backup server state.
        /// </summary>
        private void UpdateBackupServerState()
        {
            //TODO: Update backup server state here
            //TODO: This include updating data sets and updating active components.
        }

        /// <summary>
        /// Registers backoup server.
        /// </summary>
        private void RegisterBackupServer()
        {
            Register register = new Register()
            {
                Type = RegisterType.CommunicationServer,
                SolvableProblems = new string[] {"DVRP"},
                ParallelThreads = 1,
                Deregister = false,
                DeregisterSpecified = false
            };
            try
            {
                var response = _backupClient.SendRequests(new Message[] {register});
                //TODO: Set status timeout
                //TODO: This should be read from RegisterResponse message.
                BackupServerStatusInterval = 5000; //5sec
            }
            catch (SocketException) //probably Exception might be written here
            {
                //TODO: Exception caugth. Something went wrong, so we should react here
                //TODO: Default reaction would be critical ecit here.
                throw;
            }
        }

        /// <summary>
        /// Sends backup server Status messages.
        /// </summary>
        private void SendBackupStatusMessages()
        {
            while (_isWorking)
            {
                Status status = new Status()
                {
                    Threads = new StatusThread[1],
                    Id= 1,
                    
                };
                _backupClient.SendRequests(new Message[] {status});
                Thread.Sleep((int)BackupServerStatusInterval);
            }
        }
    }
}