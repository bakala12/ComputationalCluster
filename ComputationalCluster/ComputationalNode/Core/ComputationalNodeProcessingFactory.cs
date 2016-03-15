using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputationalNode.Core
{
    public interface IComputationalNodeProcessingFactory
    {
        IComputationalNodeProcessing Create();
    }
    public class ComputationalNodeProcessingModuleFactory : IComputationalNodeProcessingFactory
    {
        private static ComputationalNodeProcessingModuleFactory instance = 
            new ComputationalNodeProcessingModuleFactory();

        public static ComputationalNodeProcessingModuleFactory Factory
        {
            get
            {
                return instance;
            }
        }

        public IComputationalNodeProcessing Create()
        {
            return new ComputationalNodeProcessingModule();
        }
    }
}
