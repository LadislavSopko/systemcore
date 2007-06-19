using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace System.Core.Logging
{
    /// <summary>
    /// Represents a log entry in the log queue.
    /// </summary>
    internal class LogItem
    {
        #region Member Variables

        /// <summary>
        /// The exeception to be logged
        /// </summary>
        private Exception mException;

        /// <summary>
        /// The used defined message to be logged
        /// </summary>
        private string mMessage;

        /// <summary>
        /// The severity of the message to be logged
        /// </summary>
        private LogLevel mLogLevel;

        /// <summary>
        /// The "friendly" assembly name of the entering assembly
        /// </summary>
        private string mAssemblyName;

        /// <summary>
        /// The date and time for this log item
        /// </summary>
        private DateTime mLogDate;

        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new instance of the <see cref="LogDotNet.LogItem"/> class
        /// </summary>
        /// <param name="logLevel">The loglevel(severity) of the logitem</param>
        /// <param name="message">The used defined message</param>
        /// <param name="exception">The exception to be logged</param>
        internal LogItem(LogLevel logLevel, string message, Exception exception)
        {
            mLogDate = DateTime.Now;
            mLogLevel = logLevel;
            mMessage = message;
            mException = exception;
            mAssemblyName = Assembly.GetEntryAssembly().GetName().Name;
        }

        #endregion

        #region Internal Properties
        /// <summary>
        /// Gets the logging level(severity)
        /// </summary>
        public LogLevel LogLevel
        {
            get
            {
                return mLogLevel;
            }
        }

        /// <summary>
        /// Gets the "friendly" assembly name of the entering assembly
        /// </summary>
        public string AssemblyName
        {
            get
            {
                return mAssemblyName;
            }
        }

        /// <summary>
        /// Gets the user defined message 
        /// </summary>
        public string Message
        {
            get
            {
                return mMessage;
            }
        }

        /// <summary>
        /// Gets the associated exception
        /// </summary>
        public Exception Exception
        {
            get
            {
                return mException;
            }
        }

        /// <summary>
        /// Gets the date and time of the creatin of this logitem
        /// </summary>
        public DateTime LogDate
        {
            get
            {
                return mLogDate;
            }
        }

        #endregion

        #region Overridden Methods

        /// <summary>
        /// Provides a default string representation of the <see cref="LogDotNet.LogItem"/> class
        /// </summary>
        /// <returns>A string repesentation of the <see cref="LogDotNet.LogItem"/>class</returns>
       
        public override string ToString()
		{			
            StringBuilder sb = new StringBuilder();
			
            sb.AppendFormat("Severity: {0} Application: {1} Date{2}: ", mLogLevel.ToString().ToUpper(),mAssemblyName,mLogDate.ToString());
            sb.AppendFormat("Message: {0} ", mMessage);
            
            if (mException != null)
            {
                sb.AppendLine();
                sb.AppendFormat("Exception: {0} ", mException.ToString());
            }
			return sb.ToString();
		}
       

        #endregion
    }
}
