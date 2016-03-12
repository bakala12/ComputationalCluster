using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationsUtils.Misc
{
    public static class ArrayEx
    {
        /// <summary>
        /// generic extension method for array type
        /// </summary>
        /// <typeparam name="T">type of array</typeparam>
        /// <param name="arr">source array</param>
        /// <param name="start">subarray start index</param>
        /// <param name="end">subarray end index</param>
        /// <returns>subarray of array</returns>
        public static T[] GetSubarray<T> (this T[] arr, int start, int end)
        {
            if (end < start)
                throw new Exception("Wrong arguments");

            T[] result = new T[end - start + 1];
            Array.Copy(arr, start, result, 0, end - start + 1);
            return result;
        }
    }
}
