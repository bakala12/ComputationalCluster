using System;

namespace CommunicationsUtils.Shared
{
    public static class StringExtensions
    {
        public static T ChangeType<T>(this string obj)
        {
            try
            {
                return (T)Convert.ChangeType(obj, typeof(T));
            }
            catch (Exception)
            {
                Console.WriteLine("Parsing arguments failed.");
                throw;
            }
        }
    }
}