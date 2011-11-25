using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Web;

namespace ApprovaFlow.Utils
{
    public static class Enforce
    {   
        public static T ArgumentNotNull<T>(T argument, string description)
            where T : class
        {
            if (argument == null)
                throw new ArgumentNullException(description);

            return argument;
        }

        public static T ArgumentGreaterThanZero<T>(T argument, string description)            
        {
            if (System.Convert.ToInt32(argument) < 1)
                throw new ArgumentOutOfRangeException(description);

            return argument;
        }

        public static DateTime ArgumentDateIsInitialized(DateTime argument, string description)           
        {
            if (argument == DateTime.MinValue)
                throw new ArgumentException("DateTime has not been initialized");

            return argument;
        }

        public static Dictionary<T, K> ContainsKey<T,K>(Dictionary<T, K>search, 
                                                                T key,
                                                                string description)
        {
            if(search.ContainsKey(key) == false)
            {
                throw new KeyNotFoundException(description);
            }

            return search;
        }

        public static void That(bool condition, string message)
        {
            if (condition == false)
            {
                throw new ArgumentException(message);
            }
        }

        public static void That(bool condition, string message, List<string> errorList)
        {
            if (condition == false)
            {
                errorList.Add(message);
            }
        }
    }
}
