using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Configuration;
using System.Reflection;


namespace System.Common.Logging
{
	/// <summary>
    /// <b>This is the only class you will ever need to perform logging in your application.</b>
	/// </summary>
    /// <example>
    /// The following example shows how to perform logging in your applications.
    /// <code>
    /// public int ParseString(string number)
    /// {
    ///     int myNumber;
    ///     try
    ///     {
    ///         myNumber = int.Parse(number);
    ///         return myNumber;
    ///     }
    ///     catch (FormatException ex)
    ///     {
    ///         Logger.Instance.Debug("Failed to parse the number " + number , ex);
    ///         return 0;
    ///     }
    /// }
    /// </code>
    /// 
    /// As you can see there is no information regarding the destination of the log message.
    /// This is done by configuring the application(app.config) you performing logging from.
    /// First you need to create a new configSection in your application configuration file.
    /// <code>
    /// <![CDATA[
    /// <configSections>
    ///     <section name="LogDotNet" type="LogDotNet.LogDotNetConfigurationHandler, LogDotNet"/>
    /// </configSections>
    /// ]]>
    /// </code>
    /// LogDotNet uses the term <b>Sink</b> to describe the destination of the log message.
    /// <para></para>
    /// <list type="table">
    /// <listheader><b>The following sinks are currently supported</b></listheader>
    /// <item>
    /// <term><see cref="LogDotNet.ConsoleLogSink"/></term>
    /// <description>Sinks a log message to the console window.</description>
    /// </item>
    /// <item>
    /// <term><see cref="LogDotNet.EventLogSink"/></term>
    /// <description>Sinks a log message to the Windows EventLog</description>
    /// </item>
    /// <item>
    /// <term><see cref="LogDotNet.FileLogSink"/></term>
    /// <description>Sinks a log message to a text file</description>
    /// </item>
    /// <item>
    /// <term><see cref="LogDotNet.SmtpLogSink"/></term>
    /// <description>Sinks a log message as a mail message</description>
    /// </item>
    /// <item>
    /// <term><see cref="LogDotNet.TcpLogSink"/></term>
    /// <description>Sinks a log message onto the network making at available from ex telnet</description>
    /// </item>
    /// <item>
    /// <term><see cref="LogDotNet.DatabaseLogSink"/></term>
    /// <description>Sinks a log message to a database table</description>
    /// </item>
    /// </list>
    /// <para></para>
    /// This shows how to configure your application to sink a log message to a text file.
    /// <code>
    /// <![CDATA[
    /// <LogDotNet>
    ///     <Sink name ="MyApplicationLogName" loglevel ="Error" type ="LogDotNet.FileLogSink, LogDotnet">
    ///         <logpath value ="C:\MyApplication\Log"/>
    ///     </Sink>
    /// </LogDotNet>
    /// ]]>
    /// </code>
    /// <para></para>
    /// Note the <b>loglevel</b> value in the configuration. This is the logging threshold.
    /// In this example it means that the severity of the log message has to be at least of category <b>Error</b>.
    /// Log messages logged with <see cref="Logger.Debug(string)"/>, <see cref="Logger.Info(string)"/> and <see cref="Logger.Warning(string)"/>,
    /// will NOT be taken under concideration for this configured sink.
    /// </example>
    /// 
    /// 
    /// <remarks>
    /// 
    /// LogDotnet provides different log methods that allows you to define the severity of the log message.
    /// <list type="number">
    /// <item>
    /// <term>Debug</term><description>Logs a debug message</description>
    /// </item>
    /// <item>
    /// <term>Info</term><description>Logs a information message</description>
    /// </item>
    /// <item>
    /// <term>Warning</term><description>Logs a warning message</description>
    /// </item>
    /// <item>
    /// <term>Error</term><description>Logs a error message</description>
    /// </item>
    /// <item>
    /// <term>Fatal</term><description>Logs a fatal error message</description>
    /// </item>
    /// </list>
    /// <para>
    /// </para>
    /// <b>A note about thread safety</b>
    /// <para></para>
    /// LogDotNet is thread safe, but you should avoid two sinks accessing the same log file.
    /// Each sink runs on its separate thread to avoid blocking the main application or each other for that matter.
    /// Each sink has its own queue that the sinkthread dequeues when new logitems arrive.
    /// </remarks>
    public class Logger : ILogger
    {
        #region Member Variables
        
        /// <summary>
        /// A reference to an instance of the <see cref="Logger"/> class
        /// </summary>
        private static readonly Logger mInstance = new Logger();
        
        /// <summary>
        /// A collection of configured sinks
        /// </summary>
        private static Dictionary<string, LogSink> mSinks = new Dictionary<string, LogSink>();
        
        #endregion 
        
        #region Constructors
        /// <summary>
        /// Initializes the <see cref="Logger"/> class by reading the configured sinks from the application configuration file
        /// </summary>
        static Logger()
        {
            CreateLogSinks();
        }

        /// <summary>
        /// Private constructor to avoid creating multible instances of this class
        /// </summary>
        internal Logger()
        {
            
        }

        #endregion

        #region Public Properties
        /// <summary>
        /// Returns a single(singleton) reference to an instance of a <see cref="Logger"/> class
        /// </summary>
		public static Logger Instance
		{
			get
			{
				return mInstance;
			}
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Creates a new <see cref="LogItem"/> and distributes it to the configured sinks
        /// </summary>
        /// <param name="logLevel">The <see cref="LogLevel"/> of the message</param>
        /// <param name="message">The usedefined message to log</param>
        /// <param name="exception">The exception to log"</param>
		private void Log(LogLevel logLevel,string message, Exception exception)
		{
            try
            {
                LogItem logItem;
                foreach (LogSink logSink in mSinks.Values)
                {
                    if (logLevel >= logSink.LogLevel)
                    {
                        logItem = new LogItem(logLevel, message, exception);
                        logSink.Sink(logItem);
                    }
                }
            }
            catch { }
            
		}

        /// <summary>
        /// Creates instances of classes derived from <see cref="LogSink"/> configured in the application configuration file
        /// </summary>
        private static void CreateLogSinks()
        {
            List<SinkConfigurationElement> configuredSinks = (List<SinkConfigurationElement>)ConfigurationManager.GetSection("LogDotNet");
            if (configuredSinks != null)
            {
                foreach (SinkConfigurationElement configuredSink in configuredSinks)
                {
                    //Load the type from the library
                    Type sinkType = Type.GetType(configuredSink.Type, true, true);

                    //Create an instance of the type
                    LogSink sink = Activator.CreateInstance(sinkType) as LogSink;

                    sink.Name = configuredSink.Name;
                    sink.LogLevel = (LogLevel)Enum.Parse(typeof(LogLevel), configuredSink.LogLevel);

                    PropertyInfo[] p = sinkType.GetProperties();
                    //set the properies from the configuration
                    foreach (KeyValuePair<string, string> configuredProperty in configuredSink.Properties)
                    {
                        PropertyInfo property = sinkType.GetProperty(configuredProperty.Key, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        if (property != null)
                        {
                            object propertyValue = Convert.ChangeType(configuredProperty.Value, property.PropertyType);
                            property.SetValue(sink, propertyValue, null);
                        }
                    }
                    //Add the sink to the sinks collection
                    mSinks.Add(sink.Name, (LogSink)sink);
                    //Start the sink
                    sink.Start();
                }
            }
        }

        #endregion

        /// <summary>
        /// Returns a reference to a configured sink by its name
        /// </summary>
        /// <param name="name">The name of the sink to get</param>
        /// <returns>An instance of a class derived from <see cref="LogSink"/></returns>
        public LogSink GetSink(string name)
        {
            return mSinks[name];
        }



        #region Public Methods

        /// <summary>
        /// Logs a message with severity level <see cref="LogLevel.Debug"/> 
        /// </summary>
        /// <param name="message">The log message to log</param>
		public void Debug(string message)
		{
			Log(LogLevel.Debug, message,null);
		}

        /// <summary>
        /// Logs a message with severity level <see cref="LogLevel.Debug"/> along with a exception
        /// </summary>
        /// <param name="message">Log message to log</param>
        /// <param name="exception">Exception to be logged along with the <paramref name="message"/></param>
		public void Debug(string message, Exception exception)
		{
            Log(LogLevel.Debug, message,exception);
		}

        /// <summary>
        /// Logs a message with severity level <see cref="LogLevel.Info"/> 
        /// </summary>
        /// <param name="message">The log message to log</param>
		public void Info(string message)
		{
            Log(LogLevel.Info, message, null);
		}

        /// <summary>
        /// Logs a message with severity level <see cref="LogLevel.Info"/> along with a exception
        /// </summary>
        /// <param name="message">Log message to log</param>
        /// <param name="exception">Exception to be logged along with the <paramref name="message"/></param>
		public void Info(string message, Exception exception)
		{
            Log(LogLevel.Info, message, exception);
		}

        /// <summary>
        /// Logs a message with severity level <see cref="LogLevel.Warning"/> 
        /// </summary>
        /// <param name="message">The log message to log</param>
		public void Warning(string message)
		{
            Log(LogLevel.Warning, message, null);
		}

        /// <summary>
        /// Logs a message with severity level <see cref="LogLevel.Warning"/> along with a exception
        /// </summary>
        /// <param name="message">Log message to log</param>
        /// <param name="exception">Exception to be logged along with the <paramref name="message"/></param>
		public void Warning(string message, Exception exception)
		{
            Log(LogLevel.Warning, message, exception);
		}

        /// <summary>
        /// Logs a message with severity level <see cref="LogLevel.Error"/> 
        /// </summary>
        /// <param name="message">The log message to log</param>
		public void Error(string message)
		{
            Log(LogLevel.Error, message, null);
		}

        /// <summary>
        /// Logs a message with severity level <see cref="LogLevel.Error"/> along with a exception
        /// </summary>
        /// <param name="message">Log message to log</param>
        /// <param name="exception">Exception to be logged along with the <paramref name="message"/></param>
		public void Error(string message, Exception exception)
		{
            Log(LogLevel.Error, message, exception);
		}

        /// <summary>
        /// Logs a message with severity level <see cref="LogLevel.Fatal"/> 
        /// </summary>
        /// <param name="message">The log message to log</param>
		public void Fatal(string message)
		{
            Log(LogLevel.Fatal, message, null);
		}

        /// <summary>
        /// Logs a message with severity level <see cref="LogLevel.Fatal"/> along with a exception
        /// </summary>
        /// <param name="message">Log message to log</param>
        /// <param name="exception">Exception to be logged along with the <paramref name="message"/></param>
		public void Fatal(string message, Exception exception)
		{
            Log(LogLevel.Fatal, message, exception);
        }

        #endregion
    }
}
