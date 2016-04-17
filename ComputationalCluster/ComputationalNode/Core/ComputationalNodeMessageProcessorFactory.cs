using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UCCTaskSolver;

namespace ComputationalNode.Core
{
    public interface IComputationalNodeProcessingFactory
    {
        ComputationalNodeMessageProcessor Create(List<string> problems);
    }
    public class ComputationalNodeProcessingModuleFactory : IComputationalNodeProcessingFactory
    {
        private static ComputationalNodeProcessingModuleFactory _instance;
        private static readonly object SyncRoot = new object();

        private ComputationalNodeProcessingModuleFactory() { }

        public static ComputationalNodeProcessingModuleFactory Factory
        {
            get
            {
                lock (SyncRoot)
                {
                    if (_instance == null)
                    {
                        _instance = new ComputationalNodeProcessingModuleFactory();
                    }
                }
                return _instance;
            }
        }

        public ComputationalNodeMessageProcessor Create(List<string> problems)
        {
            return new ComputationalNodeMessageProcessor(problems);
        }
    }
}
