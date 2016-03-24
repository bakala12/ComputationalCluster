using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net.Appender;
using log4net.Core;
using log4net.Repository.Hierarchy;

namespace CommunicationsUtils.Log4Net
{
    public static class LogHelper
    {
        public static void EnableConsoleLogging()
        {
            Hierarchy hier = log4net.LogManager.GetRepository() as Hierarchy;
            if (hier != null)
            {
                var consoleAppender =
                    (ConsoleAppender)hier.GetAppenders(
                        ).FirstOrDefault(appender => appender.Name.Equals("ConsoleAppender", StringComparison.InvariantCultureIgnoreCase));

                if (consoleAppender != null)
                {
                    consoleAppender.Threshold = Level.Debug;
                    consoleAppender.ActivateOptions();
                }
            }
        }
    }
}
