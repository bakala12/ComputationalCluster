using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Core
{
    public interface IClientNodeProcessingFactory
    {
        IClientNodeProcessing Create();
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

        public IClientNodeProcessing Create()
        {
            return new ClientNodeProcessingModule();
        }
    }
}
