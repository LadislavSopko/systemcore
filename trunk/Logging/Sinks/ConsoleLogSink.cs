using System;
using System.Collections.Generic;
using System.Text;

namespace System.Core.Logging
{
    /// <summary>
    /// Writes a <see cref="LogItem"/> to a the console window
    /// </summary>
    /// <example>
    /// The following example show how to configure the <see cref="ConsoleLogSink"/> class
    /// <code>
    /// <![CDATA[
    /// <LogDotNet>
    ///     <Sink name ="MyApplicationLogName" loglevel ="Debug" type ="LogDotNet.ConsoleLogSink, LogDotnet"/>
    /// </LogDotNet>
    /// ]]>
    /// </code>
    /// </example>
    /// <remarks>
    /// Use this class to enable logging to the console window in the VS IDE
    /// </remarks>
    internal class ConsoleLogSink : LogSink
    {
        #region Overridden Methods
        /// <summary>
        /// Logs the <see cref="LogItem"/> to the console window
        /// </summary>
        /// <param name="logItem"></param>
        internal override void WriteLog(LogItem logItem)
        {
            base.WriteLog(logItem);
        }
        #endregion
    }
}
