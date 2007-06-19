using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace System.Core.Logging
{
    class EventLogSink : LogSink
    {
        /// <summary>
        /// The name of the log  
        /// </summary>
        private string mLogName = "Application";
        
        internal override void WriteLog(LogItem logItem)
        {
            
            //Check to see if this log name exists in the event log
            if (!EventLog.SourceExists(logItem.AssemblyName))
            {
                EventLog.CreateEventSource(logItem.AssemblyName, mLogName);    
            }


            EventLog.WriteEntry(logItem.AssemblyName, logItem.ToString(), TranslateLogLevel(logItem.LogLevel));
        }

        
        /// <summary>
        /// Sets or gets the name of the log to use when logging to the event log
        /// </summary>
        /// <remarks></remarks>
        public string LogName
        {
            get { return mLogName; }
            set { mLogName = value; }
        }
	


        private EventLogEntryType TranslateLogLevel(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Debug:
                    return EventLogEntryType.Information;
                    
                case LogLevel.Info:
                    return EventLogEntryType.Information;
                    
                case LogLevel.Warning:
                    return EventLogEntryType.Warning;
                    
                case LogLevel.Error:
                    return EventLogEntryType.Error;
                    
                case LogLevel.Fatal:
                    return EventLogEntryType.FailureAudit;
                    
                default:
                    return EventLogEntryType.Error;
            }
        }
    }
}
