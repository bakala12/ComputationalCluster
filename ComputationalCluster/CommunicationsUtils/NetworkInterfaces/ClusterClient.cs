using CommunicationsUtils.Messages;
using CommunicationsUtils.Serialization;
using CommunicationsUtils.NetworkInterfaces.Adapters;
using System;
using CommunicationsUtils.NetworkInterfaces.Factories;

namespace CommunicationsUtils.NetworkInterfaces
{
    /// <summary>
    /// Computational Cluster Client component.
    /// Sends messages and retrieves response messages.
    /// </summary>
    public class ClusterClient : IClusterClient
    {
        IClientAdapterFactory _tcpFactory;
        private readonly MessageToBytesConverter _converter = new MessageToBytesConverter();
        private string _address;
        private int _port;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address">Address of the server</param>
        /// <param name="port">Port of the server</param>
        public ClusterClient(string address, int port, IClientAdapterFactory tcpFactory)
        {
            _address = address;
            _port = port;
            _tcpFactory = tcpFactory;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="requests">Requests to send</param>
        /// <returns>Responses from the server</returns>
        public Message[] SendRequests (Message[] requests)
        {
            byte[] requestsBytes;
            int count = _converter.MessagesToBytes(out requestsBytes, requests);

            ITcpClient _tcpClient = _tcpFactory.Create();
            _tcpClient.Connect(_address, _port);
            using (INetworkStream networkStream = _tcpClient.GetStream())
            {
                Console.WriteLine("Client: send requests");
                networkStream.Write(requestsBytes, count);
                Console.WriteLine("Client: request sending finished");
                byte[] responseBytes = new byte[Properties.Settings.Default.MaxBufferSize];
                Console.WriteLine("Client: reading responses");
                int len = networkStream.Read(responseBytes, Properties.Settings.Default.MaxBufferSize);
                Console.WriteLine("Client: reading responses finished.");
                networkStream.Close();
                _tcpClient.Close();
                return _converter.BytesToMessages(responseBytes, len);
            }
        }
    }
}