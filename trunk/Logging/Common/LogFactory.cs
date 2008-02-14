using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace System.Common.Logging
{
    public static class LogFactory
    {

        private static Hashtable _loggers = Hashtable.Synchronized(new Hashtable());

        public static ILogger CreateLogger(string name)
        {
            Logger logger = new Logger();
            return null;
        }

        public static ILogger CreateLogger(Type type)
        {
            return null;
        }
    }
}
