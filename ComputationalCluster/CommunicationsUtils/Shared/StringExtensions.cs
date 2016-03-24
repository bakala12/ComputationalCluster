using System;
using log4net;

namespace CommunicationsUtils.Shared
{
    public static class StringExtensions
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static T ChangeType<T>(this string obj)
        {
            try
            {
                return (T)Convert.ChangeType(obj, typeof(T));
            }
            catch (Exception)
            {
                log.Debug("Parsing arguments failed.");
                
                throw;
            }
        }
    }
}