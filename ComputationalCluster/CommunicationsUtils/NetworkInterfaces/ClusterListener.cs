using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using CommunicationsUtils.Messages;
using CommunicationsUtils.Serialization;

namespace CommunicationsUtils.NetworkInterfaces
{
    /// <summary>
    /// Computational Cluster Listener component (server-side)
    /// Provides configuration, sending and retrieving messages
    /// </summary>
    public class ClusterListener
    {
        private TcpListener tcpListener;
        private int port;
        private string address;
        private MessageToBytesConverter converter = new MessageToBytesConverter();
        private Socket currentSocket = null;

        public ClusterListener(string _address, int _port)
        {
            address = _address;
            port = _port;
            IPAddress ip = IPAddress.Parse(_address);
            tcpListener = new TcpListener(ip, port);
        }

        public void StartListening ()
        {
            tcpListener.Start();
        }

        /// <summary>
        /// waits for connection, then receives request(s)
        /// </summary>
        /// <returns>messages passed to socket</returns>
        public Message[] WaitForRequest ()
        {
            currentSocket = tcpListener.AcceptSocket();
            byte[] requestBytes = new byte[Properties.Settings.Default.MaxRequestSize];
            currentSocket.Receive(requestBytes);
            return converter.BytesToMessages(requestBytes);
        }

        /// <summary>
        /// sends response(s) via open connection
        /// </summary>
        /// <param name="responses"></param>
        public void SendResponse (Message[] responses)
        {
            if (currentSocket == null)
            {
                throw new Exception("Something went wrong with connection (socket)");
            }
            byte[][] responsesBytes = converter.MessagesToBytes(responses);

            for (int i = 0; i < responsesBytes.GetLength(0); i++)
            {
                currentSocket.Send(responsesBytes[i]);
                currentSocket.Send(new byte[] { 23 });
            }

            currentSocket.Close();
            currentSocket = null;
        }
    }
}
