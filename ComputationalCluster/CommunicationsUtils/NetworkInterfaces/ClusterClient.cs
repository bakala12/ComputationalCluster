using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Sockets;
using CommunicationsUtils.Messages;
using CommunicationsUtils.Serialization;
using CommunicationsUtils.Misc;
using System.Collections.Generic;

namespace CommunicationsUtils.NetworkInterfaces
{
    /// <summary>
    /// Computational Cluster Client component.
    /// Sends messages and retrieves response messages.
    /// </summary>
    public class ClusterClient
    {
        private MessageToBytesConverter converter = new MessageToBytesConverter();
        private string address;
        private int port;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_address">Address of the server</param>
        /// <param name="_port">Port of the server</param>
        public ClusterClient(string _address, int _port)
        {
            address = _address;
            port = _port;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="requests">Requests to send</param>
        /// <returns>Responses from the server</returns>
        public Message[] SendRequests (Message[] requests)
        {
            TcpClient tcpClient = new TcpClient();
            tcpClient.Connect(address, port);
            Stream networkStream = tcpClient.GetStream();

            byte[][] bytes = converter.MessagesToBytes(requests);

            for (int i=0; i< bytes.GetLength(0); i++)
            {
                networkStream.Write(bytes[i], 0, bytes[i].Length);
                networkStream.Write(new byte[] { 23 }, 0, 1);
            }

            byte[] responseBytes = new byte[Properties.Settings.Default.MaxResponseSize];
            networkStream.Read(responseBytes, 0, Properties.Settings.Default.MaxResponseSize);
            tcpClient.Close();

            return converter.BytesToMessages(responseBytes);
        }
    }
}