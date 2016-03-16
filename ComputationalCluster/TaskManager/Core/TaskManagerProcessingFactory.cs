using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Core
{
    public interface ITaskManagerProcessingFactory
    {
        TaskManagerProcessingModule Create(List<string> problems);
    }

    /// <summary>
    /// creates TaskManagerProcessingModule instances
    /// </summary>
    public class TaskManagerProcessingModuleFactory : ITaskManagerProcessingFactory
    {
        private static TaskManagerProcessingModuleFactory instance =
    new TaskManagerProcessingModuleFactory();

        public static TaskManagerProcessingModuleFactory Factory
        {
            get
            {
                return instance;
            }
        }

        public TaskManagerProcessingModule Create(List<string> problems)
        {
            var storage = new TaskManagerStorage();
            return new TaskManagerProcessingModule(problems, storage);
        }
    }
}
