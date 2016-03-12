using CommunicationsUtils.Messages;
using CommunicationsUtils.Serialization;
using CommunicationsUtils.NetworkInterfaces.Adapters;

namespace CommunicationsUtils.NetworkInterfaces
{
    /// <summary>
    /// Computational Cluster Client component.
    /// Sends messages and retrieves response messages.
    /// </summary>
    public class ClusterClient : IClusterClient
    {
        ITcpClient _tcpClient;
        private readonly MessageToBytesConverter _converter = new MessageToBytesConverter();
        private string _address;
        private int _port;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address">Address of the server</param>
        /// <param name="port">Port of the server</param>
        public ClusterClient(string address, int port, ITcpClient tcpClient)
        {
            _address = address;
            _port = port;
            _tcpClient = tcpClient;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="requests">Requests to send</param>
        /// <returns>Responses from the server</returns>
        public Message[] SendRequests (Message[] requests)
        {
            _tcpClient.Connect(_address, _port);
            using (INetworkStream networkStream = _tcpClient.GetStream())
            {
                byte[][] bytes = _converter.MessagesToBytes(requests);

                for (int i = 0; i < bytes.GetLength(0); i++)
                {
                    networkStream.Write(bytes[i], 0, bytes[i].Length);
                    networkStream.Write(new byte[] { 23 }, 0, 1);
                }

                byte[] responseBytes = new byte[Properties.Settings.Default.MaxResponseSize];
                networkStream.Read(responseBytes, 0, Properties.Settings.Default.MaxResponseSize);

                return _converter.BytesToMessages(responseBytes);
            }
        }
    }
}