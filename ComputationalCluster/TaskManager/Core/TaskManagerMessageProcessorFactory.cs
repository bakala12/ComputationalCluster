using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UCCTaskSolver;

namespace TaskManager.Core
{
    public interface ITaskManagerProcessingFactory
    {
        TaskManagerMessageProcessor Create(List<string> problems);
    }

    /// <summary>
    /// creates TaskManagerProcessingModule instances
    /// </summary>
    public class TaskManagerMessageProcessorFactory : ITaskManagerProcessingFactory
    {
        private static TaskManagerMessageProcessorFactory instance =
    new TaskManagerMessageProcessorFactory();

        public static TaskManagerMessageProcessorFactory Factory
        {
            get
            {
                return instance;
            }
        }

        public TaskManagerMessageProcessor Create(List<string> problems)
        {
            var storage = new TaskManagerStorage();
            return new TaskManagerMessageProcessor(problems, storage);
        }
    }
}
