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
    //common things for client components (that means TM and CN, but NO CC)
    public abstract class ClientComponent: IClusterComponent
    {
        protected IClusterClient clusterClient;
        protected ulong componentId;
        protected uint timeout;

        public ClientComponent(IClusterClient _clusterClient)
        {
            clusterClient = _clusterClient;
        }

        protected abstract void registerComponent();
        public abstract void Run();
        public abstract void updateBackups(NoOperation msg);

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
                        updateBackups(response.Cast<NoOperation>());
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
