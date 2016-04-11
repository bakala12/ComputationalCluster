using CommunicationsUtils.Messages;
using CommunicationsUtils.NetworkInterfaces;
using CommunicationsUtils.NetworkInterfaces.Adapters;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace CommunicationsUtils.ClientComponentCommon
{
    //common things for cluster's internal client components (TM and CN, not comp. client)
    public abstract class InternalClientComponent: ExternalClientComponent
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        //external:
        //run defined in inheriting classes
        //updateBackups defined in inherited class

        //internal:
        #region fields
        /// <summary>
        /// tcp client wrapper used in sending problem related messages only
        /// </summary>
        protected IClusterClient problemClient;
        /// <summary>
        /// lock to handle possible undesirable concurrent writing via problemClient
        /// </summary>
        protected readonly object SyncRoot = new object();
        /// <summary>
        /// creates Message[] array from params messages (test-friendly feature)
        /// </summary>
        protected IMessageArrayCreator creator;
        /// <summary>
        /// contains queue of responses from server
        /// </summary>
        protected ConcurrentQueue<Message> messageQueue;
        /// <summary>
        /// timeout intialized in register procedures
        /// </summary>
        protected uint timeout;
        /// <summary>
        /// unique component's cluster id assigned in register procedures
        /// </summary>
        protected ulong componentId;
        #endregion

        public InternalClientComponent
            (IClusterClient _clusterClient, IClusterClient _problemClient,
            IMessageArrayCreator _creator) : base (_clusterClient)
        {
            problemClient = _problemClient;
            creator = _creator;
            messageQueue = new ConcurrentQueue<Message>();
        }

        #region methods
        /// <summary>
        /// send register message to server
        /// (status sender thread)
        /// </summary>
        public abstract void RegisterComponent();


        /// <summary>
        /// responses handler
        /// starts new long running computations' threads, dequeues messages
        /// (message processor thread)
        /// </summary>
        public abstract void HandleResponses();

        /// <summary>
        /// send status message to server
        /// (status sender thread)
        /// </summary>
        /// <returns></returns>
        public abstract Message[] SendStatus();

        /// <summary>
        /// function from which new long-running problem-related thread starts
        /// enters computations (given by computationFunction), sends solutions 
        /// </summary>
        /// <param name="computationFunction"></param>
        public void StartLongComputation(Func<Message> computationFunction)
        {
            Message m = computationFunction.Invoke();
            if (m != null)
                SendProblemRelatedMessage(m);
        }

        /// <summary>
        /// sends some problem related message via problemClient
        /// </summary>
        /// <param name="request"></param>
        public void SendProblemRelatedMessage(Message request)
        {
            Message[] requests = creator.Create(request);
            log.DebugFormat("Sending after computations: {0}", request.ToString());
            Message[] responses;
            lock (SyncRoot)
            {
                responses = this.SendMessages(problemClient, requests);
            }
            foreach (var response in responses)
                messageQueue.Enqueue(response);
        }

        /// <summary>
        /// common for TM and CN register response handler
        /// e.g. assigns componentId, timeout, ...
        /// (status sender thread)
        /// </summary>
        /// <param name="registerMessage"></param>
        protected virtual void handleRegisterResponses(Register registerMessage)
        {
            Message[] requests = creator.Create(registerMessage);
            Message[] responses = this.SendMessages(clusterClient, requests);
            RegisterResponse registerResponse = null;
            foreach (var response in responses)
            {
                switch (response.MessageType)
                {
                    case MessageType.RegisterResponseMessage:
                        if (registerResponse != null)
                            throw new Exception("Multiple register responses");
                        log.Debug("RegisterResponse acquired: updating fields...");
                        registerResponse = response.Cast<RegisterResponse>();
                        break;
                    case MessageType.NoOperationMessage:
                        log.Debug("NoOperation acquired: updating backups");
                        UpdateBackups(response.Cast<NoOperation>());
                        break;
                    default:
                        throw new Exception("Invalid message delivered in register procedure "
                            + response.ToString());
                }
            }

            if (registerResponse == null)
                throw new Exception("No register response ");

            componentId = registerResponse.Id;
            timeout = registerResponse.Timeout;
        }

        public override Message[] SendMessages(IClusterClient client, Message[] requests)
        {
            client.ChangeListenerParameters(currentAddress, currentPort);
            return base.SendMessages(client, requests);
        }
        #endregion
    }
}
