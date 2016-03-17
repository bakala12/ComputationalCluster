using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputationalNode.Core
{
    public interface IComputationalNodeProcessingFactory
    {
        ComputationalNodeProcessingModule Create(List<string> problems);
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

        public ComputationalNodeProcessingModule Create(List<string> problems)
        {
            return new ComputationalNodeProcessingModule(problems);
        }
    }
}
