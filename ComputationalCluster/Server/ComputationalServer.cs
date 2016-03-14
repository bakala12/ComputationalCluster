using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using CommunicationsUtils.ClientComponentCommon;
using CommunicationsUtils.Messages;
using CommunicationsUtils.NetworkInterfaces;

namespace Server
{
    public partial class ComputationalServer : IRunnable
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
        /// Initializes a new instance of ComputationalServer with the specified listener.
        /// The default state of server is Backup.
        /// </summary>
        /// <param name="listener">Listener object which handle communication.</param>
        public ComputationalServer(IClusterListener listener)
        {
            if(listener==null) throw new ArgumentNullException(nameof(listener));
            _clusterListener = listener;
            State = ServerState.Backup;
            _messagesQueue = new ConcurrentQueue<Message>();
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
        /// Server working loop.
        /// </summary>
        protected virtual void DoWork()
        {
            while (true)
            {
                Message[] requests = _clusterListener.WaitForRequest();
                foreach (var item in requests)
                {
                    _messagesQueue.Enqueue(item);
                }
                //Do sth with received messages. And send responeses. This is the main server task.
                //This can be handled by a dedicated class.
            }
        }
    }
}