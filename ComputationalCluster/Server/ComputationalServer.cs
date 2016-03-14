using System;
using System.Collections.Concurrent;
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
        /// Starts void argumentsless delegate in new thread.
        /// </summary>
        /// <param name="delegatFunc"></param>
        private static void ProcessInParallel(Action delegatFunc)
        {
            Task.Run(delegatFunc);
        }

        /// <summary>
        /// Creates array of response messages for specified message.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private static Message[] CreateResponseMessages(Message message)
        {
            //TODO
            throw new NotImplementedException();
        }

        /// <summary>
        /// Processes specified message.
        /// </summary>
        /// <param name="message"></param>
        private static void ProcessMessage(Message message)
        {
            //TODO
            // jak wywołać na listenerze SendResponse. Z kolejki mamy tylko wiadomość, nie mamy listenera, skąd go wziąć.
            throw new NotImplementedException();
            // ewentualne dodanie wiadomości spowrotem do kolejki jeżeli potrzeba

        }

        /// <summary>
        /// Delegate for listening and storing messages thread.
        /// </summary>
        private void ListenAndStoreMessages()
        {
            while (true)
            {
                var requestsMessages = _clusterListener.WaitForRequest();
                foreach (var message in requestsMessages)
                {
                    _messagesQueue.Enqueue(message);
                    // gadalismy o tym że jak komponent wysle wiadomość ze statusem że jest wolny to mozna mu odesłać wtedy coś do porobienia
                    // no ale to wychodzi na to że powinno być to w tym wątku bo on nasłuchuje i tu istnieje ten listener, a miało być w osobnym wątku,
                    // trochę crap
                    var responseMessages = CreateResponseMessages(message);
                    _clusterListener.SendResponse(responseMessages);
                }
            }
        }

        /// <summary>
        /// Delegate for dequeueing and processing messages thread.
        /// </summary>
        private void DequeueAndProcessMessages()
        {
                while (true)
                {
                    Message message;
                    var result = _messagesQueue.TryDequeue(out message);
                    if (!result) continue;
                    ProcessMessage(message);
                }
        }

        /// <summary>
        /// Server work function.
        /// </summary>
        protected virtual void DoWork()
        {
            ProcessInParallel(ListenAndStoreMessages);
            ProcessInParallel(DequeueAndProcessMessages);           
        }
    }
}