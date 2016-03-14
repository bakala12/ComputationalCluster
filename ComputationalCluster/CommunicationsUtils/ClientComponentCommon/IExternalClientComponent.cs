using CommunicationsUtils.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationsUtils.ClientComponentCommon
{
    //will be abstract class with table of backup serv information in future
    //provides methods for internal+external client components (TM+CN+CC)
    public interface IExternalClientComponent
    {
        /// <summary>
        /// basic, very general method - called in main() of component
        /// </summary>
        void Run();

        /// <summary>
        /// all of the clients store malfunction-related info
        /// </summary>
        /// <param name="msg"></param>
        void UpdateBackups(NoOperation msg);
    }
}
