using CommunicationsUtils.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationsUtils.ClientComponentCommon
{
    public interface IClusterComponent
    {
        void Run();
        void updateBackups(NoOperation msg);
    }
}
