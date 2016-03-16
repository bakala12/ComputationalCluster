using CommunicationsUtils.Messages;
using CommunicationsUtils.NetworkInterfaces;
using CommunicationsUtils.NetworkInterfaces.Adapters;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationsUtils.ClientComponentCommon
{
    //common things for cluster's internal client components (TM and CN, not comp. client)
    public abstract class InternalClientComponent: IExternalClientComponent
    {
        //external:
        public abstract void Run();
        public abstract void UpdateBackups(NoOperation msg);

        //internal:
        #region fields
        /// <summary>
        /// tcp client wrapper used in status sending thread only
        /// </summary>
        protected IClusterClient statusClient;
        /// <summary>
        /// tcp client wrapper used in sending problem related messages only
        /// </summary>
        protected IClusterClient problemClient;
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
            IMessageArrayCreator _creator)
        {
            statusClient = _clusterClient;
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
        /// sends some problem solution via problemClient
        /// </summary>
        /// <param name="request"></param>
        public void SendProblemRelatedMessage(Message request)
        {
            Message[] requests = creator.Create(request);
            Message[] responses = problemClient.SendRequests(requests);
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
            Message[] responses = statusClient.SendRequests(requests);
            RegisterResponse registerResponse = null;
            foreach (var response in responses)
            {
                switch (response.MessageType)
                {
                    case MessageType.RegisterResponseMessage:
                        if (registerResponse != null)
                            throw new Exception("Multiple register responses");
                        registerResponse = response.Cast<RegisterResponse>();
                        break;
                    case MessageType.NoOperationMessage:
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
        #endregion
    }
}
