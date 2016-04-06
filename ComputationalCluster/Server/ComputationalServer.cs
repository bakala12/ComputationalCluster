using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using CommunicationsUtils.Messages;
using CommunicationsUtils.NetworkInterfaces;
using log4net;
using CommunicationsUtils.NetworkInterfaces.Factories;
using Server.Data;
using Server.Interfaces;
using Server.MessageProcessing;

namespace Server
{
    public class ComputationalServer : IChangeServerState
    {
        /// <summary>
        /// An object used to call log methods
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
        /// List of currently available backups servers.
        /// </summary>
        private List<BackupServerInfo> _backups;

        /// <summary>
        /// Queue used by servers to synchronize data
        /// </summary>
        private readonly ConcurrentQueue<Message> _synchronizationQueue;

        /// <summary>
        /// Current state of server.
        /// </summary>
        public ServerState State { get; private set; }

        /// <summary>
        /// List of active components in the system.
        /// INDEXED by componentId - assigned by server
        /// </summary>
        private readonly ConcurrentDictionary<int, ActiveComponent> _activeComponents;

        /// <summary>
        /// List of active problem data sets.
        /// INDEXED by problemId - assigned by server
        /// </summary>
        private readonly ConcurrentDictionary<int, ProblemDataSet> _problemDataSets;


        /// <summary>
        /// Object responsible for processing messages.
        /// </summary>
        private MessageProcessor _messageProcessor;

        /// <summary>
        /// Specifies the time interval between two Status messages.
        /// </summary>
        public ulong? BackupServerStatusInterval { get; protected set; }

        /// <summary>
        /// Gets BackupServerId (if specified).
        /// </summary>
        public ulong? BackupServerId { get; protected set; }

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
            _synchronizationQueue = new ConcurrentQueue<Message>();
            _messageProcessor = (state == ServerState.Primary)
                ? new PrimaryMessageProcessor
                (_clusterListener, _synchronizationQueue, _problemDataSets, _activeComponents) as MessageProcessor
                : new BackupMessageProcessor
                (_clusterListener, _synchronizationQueue, _problemDataSets, _activeComponents);
            _backups = new List<BackupServerInfo>();
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
            Log.Debug("Creating new instance of ComputationalServer.");
        }

        /// <summary>
        /// Initializes a new instance of ComputationalServer class withe the specified listener and 
        /// a speciefied server state.
        /// </summary>
        /// <param name="backupClient"> A client used as BS request sender.</param>
        /// /// <param name="backupListener"> A listener used as BS request receiver.</param>
        public ComputationalServer(IClusterClient backupClient, IClusterListener backupListener) :this(ServerState.Backup)
        {
            if(backupClient == null) throw new ArgumentNullException(nameof(backupClient));
            _backupClient = backupClient;
            _clusterListener = backupListener;
            Log.Debug("New instance of Backup ComputationalServer has been created.");
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
            lock (_syncRoot)
            {
                _messageProcessor = new PrimaryMessageProcessor
                    (_clusterListener, _synchronizationQueue, _problemDataSets, _activeComponents);
            }
            _backupClient = null;
            if (_clusterListener == null)
                _clusterListener = ClusterListenerFactory.Factory.Create(IPAddress.Any, Properties.Settings.Default.Port);
            //_backups.Clear();

            Log.Debug("Starting listening mechanism.");
            _clusterListener.Start();
            _isWorking = true;
            _currentlyWorkingThreads.Clear();
            Log.Debug("Listening mechanism has been started.");
            BackupServerStatusInterval = null;
            BackupServerId = null;
            DoPrimaryWork();
        }

        /// <summary>
        /// Runs server as Backup server.
        /// </summary>
        public virtual void RunAsBackup()
        {
            lock (_syncRoot)
            {
                _messageProcessor = new BackupMessageProcessor
                    (_clusterListener, _synchronizationQueue, _problemDataSets, _activeComponents);
            }
            if (_backupClient == null)
                _backupClient = ClusterClientFactory.Factory.Create(Properties.Settings.Default.MasterAddress,
                    Properties.Settings.Default.MasterPort);
            if (_clusterListener == null)
                _clusterListener = ClusterListenerFactory.Factory.Create(IPAddress.Any, Properties.Settings.Default.Port);
            //AZBEST314
            Log.Debug("Starting backup listening mechanism.");
            _clusterListener.Start();
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
            _backups.RemoveAt(0);
            Log.Debug("\n*** ASSUMING PRIMARY ROLE ***\n");
            Run();
        }

        /// <summary>
        /// Stops server work.
        /// </summary>
        public void Stop()
        {
            Log.Debug("Stopping threads.");
            _clusterListener.Stop();
            _clusterListener = null;
            _isWorking = false;
            foreach (var currentlyWorkingThread in _currentlyWorkingThreads)
            {
                if(currentlyWorkingThread != Thread.CurrentThread) //that's weird...
                currentlyWorkingThread?.Join();
            }
            _currentlyWorkingThreads.Clear();

            _backupClient = null;
            //Stopping and joining messageprocessor threads 
            if (State == ServerState.Backup)
            {
                _messageProcessor.Stop();
                foreach (var thread in _messageProcessor.StatusThreads)
                {
                    thread?.Join();
                }
                _messageProcessor.StatusThreads.Clear();
            }
            Log.Debug("Threads have been stopped.");
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
                    Log.Debug("Waiting for request messages.");
                    Message[] requestsMessages = null;
                    try
                    {
                        requestsMessages = _clusterListener.WaitForRequest();
                    }
                    catch (Exception)
                    {
                        Log.Debug("Communication accident. Connection has been broken down");
                        //throw;
                        return;
                    }
                    if (requestsMessages==null) Log.Debug("No request messages detected.");
                    Log.Debug("Request messages has been awaited. Numer of request messages: " + requestsMessages.Length);
                    foreach (var message in requestsMessages)
                    {
                        if (message.MessageType != MessageType.RegisterMessage &&
                            message.MessageType != MessageType.NoOperationMessage &&
                            message.MessageType != MessageType.SolutionRequestMessage &&
                            message.MessageType != MessageType.SolveRequestMessage &&
                            message.MessageType != MessageType.ErrorMessage &&
                            message.MessageType != MessageType.StatusMessage)
                        {
                            _messagesQueue.Enqueue(message);
                        }
                        Log.DebugFormat("Enqueueing {0} message.", message.MessageType);
                        var responseMessages = _messageProcessor.CreateResponseMessages(message, _problemDataSets,
                            _activeComponents, _backups);
                        _clusterListener?.SendResponse(responseMessages);
                        Log.DebugFormat("Response for {0} message has been sent.", message.MessageType);
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
                Message message;
                var result = _messagesQueue.TryDequeue(out message);
                if (!result) continue;
                Log.DebugFormat("Dequeueing {0} message.", message.MessageType);
                _messageProcessor.ProcessMessage(message, _problemDataSets, _activeComponents);
                Log.DebugFormat("Message {0} has been proccessed.", message.MessageType);
            }
        }

        /// <summary>
        /// Server work function.
        /// </summary>
        protected virtual void DoPrimaryWork()
        {
            Log.Debug("Starting new thread for dequeueing messages and updating additional sets.");
            ProcessInParallel(DequeueMessagesAndUpdateProblemStructures);
            Log.Debug("Thread for dequeueing messages and updating additional sets has been started.");
            Log.Debug("Initializing listening module in main thread");
            ListenAndStoreMessagesAndSendResponses();
        }

        /// <summary>
        /// Backup server function work.
        /// </summary>
        protected virtual void DoBackupWork()
        {
            Log.Debug("Starting server as backup");
            Log.Debug("Starting backup client");
            Log.Debug("Registering backup server");
            RegisterBackupServer();
            Log.Debug("Backup registered successfully");
            Log.Debug("Starting status thread");
            ProcessInParallel(SendBackupStatusMessages);
            Log.Debug("Starting new thread for dequeueing messages and updating additional sets.");
            ProcessInParallel(DequeueMessagesAndUpdateProblemStructures);
            Log.Debug("Starting additonal listener on backup");
            ListenAndStoreMessagesAndSendResponses();
        }

        /// <summary>
        /// Registers backoup server.
        /// </summary>
        private void RegisterBackupServer()
        {
            var register = new Register()
            {
                Type = new RegisterType()
                {
                    Value = ComponentType.CommunicationServer,
                    port = (ushort)Properties.Settings.Default.Port,
                    portSpecified = true
                },
                SolvableProblems = new[] {"DVRP"},
                ParallelThreads = 1,
                Deregister = false,
                DeregisterSpecified = false
            };
            try
            {
                var response = _backupClient.SendRequests(new Message[] {register});
                foreach (var message in response)
                {
                    _messageProcessor.ProcessMessage(message, _problemDataSets, _activeComponents);
                    if (message.MessageType == MessageType.RegisterResponseMessage)
                    {
                        var registerResponse = message.Cast<RegisterResponse>();
                        BackupServerStatusInterval = registerResponse.Timeout;
                        BackupServerId = registerResponse.Id;
                    }
                    if (message.MessageType == MessageType.NoOperationMessage)
                    {
                        NoOperation nop = message.Cast<NoOperation>();
                        //current positon of backup in hierarchy (1 for first backup, etc...)
                        int pos = nop.BackupServersInfo.Length;
                        _backups = nop.BackupServersInfo.ToList();
                        //if youre not first backup, switch listener params to higher
                        if (pos > 1)
                        {
                            _backupClient.ChangeListenerParameters
                                (_backups[pos - 2].address,_backups[pos - 2].port);
                        }
                    }
                }
            }
            catch (SocketException) //probably Exception might be written here
            {
                //TODO: Exception caugth. Something went wrong, so we should react here
                //TODO: Default reaction would be critical exit here.
                //TODO: Or maybe make this seerver primary?
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
                ulong id = BackupServerId ?? 0;
                var status = new Status()
                {
                    Threads = new StatusThread[1],
                    Id= id
                };
                try
                {
                    var response = _backupClient.SendRequests(new Message[] {status});
                    foreach (var message in response)
                    {
                        if (message.MessageType != MessageType.NoOperationMessage)
                        {
                            _synchronizationQueue.Enqueue(message);
                            _messagesQueue.Enqueue(message);
                            Log.Debug("Received message "+message.MessageType.ToString());
                            continue;
                        }

                        var nop = message.Cast<NoOperation>();
                        Log.Debug("No operation message received");
                        _backups = nop.BackupServersInfo.ToList();
                    }
                }
                catch (Exception)
                {
                    if (BackupProblem())
                    {
                        ChangeState(ServerState.Primary);
                    }
                }
                Thread.Sleep((int)(BackupServerStatusInterval!= null ? BackupServerStatusInterval/4 : 0));
            }
        }

        /// <summary>
        /// Invoked when backup server cannot connect to primary server. 
        /// Checks whether the invoking backup is the first backup server. 
        /// </summary>
        /// <returns>True if the invoking backup is the first backup server, otherwise false.</returns>
        private bool BackupProblem()
        {
            //if(_backups == null || _backups.Count ==0)
                //throw new Exception("Backup has empty backup list!!!");
            //become main server - we assume backups' invulnerability
            if (true)
            return true;
        }
    }
}