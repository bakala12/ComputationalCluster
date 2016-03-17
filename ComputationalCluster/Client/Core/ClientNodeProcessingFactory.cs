using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Core
{
    public interface IClientNodeProcessingFactory
    {
        ClientNodeProcessingModule Create();
    }

    /// <summary>
    /// creates ClienNodeProcessingModule instances
    /// </summary>
    public class ClientNodeProcessingModuleFactory : IClientNodeProcessingFactory
    {
        private static ClientNodeProcessingModuleFactory instance = 
            new ClientNodeProcessingModuleFactory();

        public static ClientNodeProcessingModuleFactory Factory
        {
            get
            {
                return instance;
            }
        }

        public ClientNodeProcessingModule Create()
        {
            return new ClientNodeProcessingModule();
        }
    }
}
