using CommunicationsUtils.Messages;
using CommunicationsUtils.NetworkInterfaces;
using CommunicationsUtils.NetworkInterfaces.Adapters;
using System;
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
        protected IClusterClient clusterClient;
        protected IClusterClient specialClusterClient;
        protected IMessageArrayCreator creator;
        protected uint timeout;
        protected ulong componentId;

        public InternalClientComponent(IClusterClient _clusterClient, IClusterClient _special,
            IMessageArrayCreator _creator)
        {
            clusterClient = _clusterClient;
            specialClusterClient = _special;
            creator = _creator;
        }

        /// <summary>
        /// send register message to server
        /// </summary>
        protected abstract void registerComponent();

        /// <summary>
        /// send status message to server
        /// </summary>
        /// <returns></returns>
        public abstract Message[] SendStatus();

        //common for TM and CN method: handles register responses
        //getting component's id, initializing backup table...
        protected virtual void handleRegisterResponses(Register registerMessage)
        {
            Message[] responses = clusterClient.SendRequests(new[] { registerMessage });
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
    }
}
