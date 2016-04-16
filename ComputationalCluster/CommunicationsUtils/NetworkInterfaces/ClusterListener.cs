using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using CommunicationsUtils.Messages;
using CommunicationsUtils.Serialization;
using CommunicationsUtils.NetworkInterfaces.Adapters;

namespace CommunicationsUtils.NetworkInterfaces
{
    /// <summary>
    /// Computational Cluster Listener component (server-side)
    /// Provides configuration, sending and retrieving messages
    /// </summary>
    public class ClusterListener : IClusterListener
    {
        private ITcpListener tcpListener;
        private MessageToBytesConverter converter = new MessageToBytesConverter();
        private ISocket currentSocket = null;

        public ClusterListener(ITcpListener listener)
        {
            tcpListener = listener;
        }

        public void Start ()
        {
            tcpListener.Start();
        }

        /// <summary>
        /// waits for connection, then receives request(s)
        /// </summary>
        /// <returns>messages passed to socket</returns>
        public Message[] WaitForRequest ()
        {
            try
            {
                currentSocket = tcpListener.AcceptSocket();
            }
            catch (Exception)
            {
                tcpListener.Start();
                currentSocket = tcpListener.AcceptSocket();
            }
            byte[] requestBytes = new byte[Properties.Settings.Default.MaxBufferSize];
            int len = currentSocket.Receive(requestBytes, Properties.Settings.Default.MaxBufferSize);
            return converter.BytesToMessages(requestBytes, len);
        }

        public string ExtractSocketAddress()
        {
            return currentSocket== null ?"": currentSocket.ExtractSocketAddress();
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
            byte[] responsesBytes;
            int count = converter.MessagesToBytes(out responsesBytes, responses);
            currentSocket.Send(responsesBytes, count);
            currentSocket.Close();
            currentSocket = null;
        }

        public void Stop ()
        {
            tcpListener.Stop();
        }

        public void KillSocket()
        {
            currentSocket.KillSocket();
        }
    }
}
