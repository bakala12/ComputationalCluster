using CommunicationsUtils.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationsUtils.ClientComponentCommon
{
    //will be abstract class with table of backup serv information in future
    //provides methods for internal+external client components (TM+CN+Comp. Client)
    public abstract class ExternalClientComponent
    {
        //there will be here something like:
        private BackupServerInfo[] _backups;

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
            _backups = msg.BackupServersInfo;
        }
    }
}
