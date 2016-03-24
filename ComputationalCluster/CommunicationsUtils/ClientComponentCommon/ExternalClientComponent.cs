using CommunicationsUtils.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommunicationsUtils.NetworkInterfaces;

namespace CommunicationsUtils.ClientComponentCommon
{
    //will be abstract class with table of backup serv information in future
    //provides methods for internal+external client components (TM+CN+Comp. Client)
    public abstract class ExternalClientComponent
    {
        /// <summary>
        /// main tcp client wrapper for client node
        /// </summary>
        protected IClusterClient clusterClient;
        //backup info table reference
        private BackupServerInfo[] backups;

        protected ExternalClientComponent(IClusterClient _clusterClient)
        {
            clusterClient = _clusterClient;
        }

        /// <summary>
        /// basic, very general method - called in main() of component
        /// </summary>
        public abstract void Run();

        /// <summary>
        /// update backups' list
        /// </summary>
        /// <param name="msg"></param>
        public void UpdateBackups(NoOperation msg)
        {
            backups = msg.BackupServersInfo;
        }

        /// <summary>
        /// backup-consider message sending subroutine
        /// not considering posibility of simultaneous 
        /// failure of main and backup server (too improbable for this project)
        /// </summary>
        /// <param name="client">cliet</param>
        /// <param name="requests"></param>
        /// <returns></returns>
        public virtual Message[] SendMessages(IClusterClient client, Message[] requests)
        {
            Message[] responses;
            try
            {
                responses = client.SendRequests(requests);
            }
            catch (Exception)
            {
                if (backups != null && backups.Length == 0)
                {
                    throw new Exception("Critical client failure. Server timeout" +
                                        " and no backups specified");
                }
                try
                {
                    client.ChangeListenerParameters(backups[0].address, backups[0].port);
                    Thread.Sleep(5000);
                    responses = client.SendRequests(requests);
                }
                catch (Exception)
                {
                    throw new Exception("Critical client failure. Server timeout " +
                                        "and primary backup timeout");
                }
            }
            return responses;
        }
    }
}
