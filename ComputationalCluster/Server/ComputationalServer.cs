﻿using System;
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
                ? new PrimaryMessageProcessor(_synchronizationQueue) as MessageProcessor
                : new BackupMessageProcessor(_synchronizationQueue);
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
            //TODO: Primary initialize
            lock (_syncRoot)
            {
                _messageProcessor = new PrimaryMessageProcessor(_synchronizationQueue);
            }
            _backupClient = null;
            if (_clusterListener == null)
                _clusterListener = ClusterListenerFactory.Factory.Create(IPAddress.Any, Properties.Settings.Default.Port);
            //TODO: Maybe reset data sets and list of active components here

            //TODO: Primary run !!!
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
            //TODO: Backup initilize here
            lock(_syncRoot)
            {
                _messageProcessor = new BackupMessageProcessor(_synchronizationQueue);
            }
            _backups.Add(new BackupServerInfo()
            {
                address = Properties.Settings.Default.MasterAddress,
                port = (ushort)Properties.Settings.Default.Port,
            });
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
            Log.Debug("Stopping threads.");
            _clusterListener.Stop();
            _isWorking = false;
            foreach (var currentlyWorkingThread in _currentlyWorkingThreads)
            {
                if(currentlyWorkingThread != Thread.CurrentThread) //that's weird...
                currentlyWorkingThread?.Join();
            }
            _currentlyWorkingThreads.Clear();
            _clusterListener = null;
            _backupClient = null;
            //TODO: Stop that thread. Not in this way, because it probabely does nothing.
            //if (State == ServerState.Backup)
              //  _messageProcessor?.StatusThread?.Join();
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
                    var requestsMessages = _clusterListener.WaitForRequest();
                    if(requestsMessages==null) Log.Debug("No request messages detected.");
                    // ReSharper disable once PossibleNullReferenceException
                    Log.Debug("Request messages has been awaited. Numer of request messages: " + requestsMessages.Length);
                    foreach (var message in requestsMessages)
                    {
                        //TODO: not all messages should be enqueued (no SolveRequest, no Register)
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
                        _clusterListener.SendResponse(responseMessages);
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
                lock (_syncRoot)
                {
                    Message message;
                    var result = _messagesQueue.TryDequeue(out message);
                    if (!result) continue;
                    Log.DebugFormat("Dequeueing {0} message.", message.MessageType);
                    _messageProcessor.ProcessMessage(message, _problemDataSets, _activeComponents);
                    Log.DebugFormat("Message {0} has been proccessed.", message.MessageType);
                }
            }
        }

        /// <summary>
        /// Server work function.
        /// </summary>
        protected virtual void DoPrimaryWork()
        {
            Log.Debug("Starting new thread for listening, storing messages and sending responses.");
            ProcessInParallel(ListenAndStoreMessagesAndSendResponses);
            Log.Debug("Thread for listening, storing messages and sending responses has been started.");
            Log.Debug("Starting new thread for dequeueing messages and updating additional sets.");
            ProcessInParallel(DequeueMessagesAndUpdateProblemStructures);
            Log.Debug("Thread for dequeueing messages and updating additional sets has been started.");
        }

        /// <summary>
        /// Backup server function work.
        /// </summary>
        protected virtual void DoBackupWork()
        {
            //TODO: Do this things
            Log.Debug("Starting server as backup");
            Log.Debug("Starting backup client");
            Log.Debug("Registering backup server");
            RegisterBackupServer();
            Log.Debug("Backup registered successfully");
            Log.Debug("Starting status thread");
            ProcessInParallel(SendBackupStatusMessages);
            //log.Debug("Starting updating backup thread");
            //ProcessInParallel(UpdateBackupServerState);
            Log.Debug("Starting new thread for dequeueing messages and updating additional sets.");
            ProcessInParallel(DequeueMessagesAndUpdateProblemStructures);
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
                    if (message.MessageType != MessageType.NoOperationMessage) continue;
                    var nop = message.Cast<NoOperation>();
                    var n = nop.BackupServersInfo.Length;
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
                var status = new Status()
                {
                    Threads = new StatusThread[1],
                    Id= 1,
                    
                };
                try
                {
                    var response = _backupClient.SendRequests(new Message[] {status});
                    foreach (var message in response)
                    {
                        //TODO: Process all response messages in backup.
                        //TODO: Update backup list!
                        _synchronizationQueue.Enqueue(message);
                        _messagesQueue.Enqueue(message);

                        if (message.MessageType != MessageType.NoOperationMessage) continue;
                        var nop = message.Cast<NoOperation>();
                        _backups = nop.BackupServersInfo.ToList();
                        //TODO: Update components. 
                        //TODO: Call synchronization functions here.
                    }
                }
                catch (Exception)
                {
                    //TODO: Switching context to primary server or rearrange backup list.
                    //TODO: Assuming _backups field contains current list of available backups. 
                    if (BackupProblem())
                    {
                        ChangeState(ServerState.Primary);
                        //TODO: Watch out on threads!!!
                    }
                }
                Thread.Sleep((int)(BackupServerStatusInterval ?? 0));
            }
        }

        /// <summary>
        /// Invoked when backup server cannot connect to primary server. 
        /// Checks whether the invoking backup is the first backup server. 
        /// </summary>
        /// <returns>True if the invoking backup is the first backup server, otherwise false.</returns>
        private bool BackupProblem()
        {
            if(_backups == null || _backups.Count ==0)
                throw new Exception("Backup has empty backup list!!!");
            foreach (var backupServerInfo in _backups)
            {
                //TODO: Check whether i'm the first backup server.
                if(true /*change this*/) //TODO: Single backup implementation.
                    return true;
            }
            return false;
        }
    }
}