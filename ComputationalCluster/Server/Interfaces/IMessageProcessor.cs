using System.Collections.Generic;
using CommunicationsUtils.Messages;
using Server.Data;

namespace Server.Interfaces
{
    public interface IMessageProcessor
    {
        void ProcessMessage(Message message, IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents);

        Message[] CreateResponseMessages(Message message, IDictionary<int, ProblemDataSet> dataSets,
            IDictionary<int, ActiveComponent> activeComponents);
    }
}
