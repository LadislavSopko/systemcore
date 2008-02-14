using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Reflection;

namespace System.Common.Logging
{
    /// <summary>
    /// Writes a <see cref="LogItem"/> to a text file
    /// </summary>
    /// <example>
    /// The following example shows how to configure the <see cref="FileLogSink"/> class
    /// <code>
    /// <![CDATA[
    /// <LogDotNet>
    ///     <Sink name ="MyApplicationLogName" loglevel ="Debug" type ="LogDotNet.FileLogSink, LogDotnet">
    ///         <logpath value ="C:\Log"/>
    ///     </Sink>
    /// </LogDotNet>
    /// ]]>
    /// </code>
    /// </example>
    /// <remarks>
    /// The <see cref="DateTimeFormat"/> property determines how long a file will be written to before creating a new log file.
    /// Note that the <see cref="LogPath"/> can be both relative or absolute to its executing assembly.
    /// </remarks>
    internal class FileLogSink : LogSink
    {

        #region Member Variables
        /// <summary>
        /// Contains the relative or absolute logging path 
        /// </summary>
        private string mLogPath = "";

        /// <summary>
        /// Contains the formatstring used to append datetime information to the filename
        /// </summary>
        private string mDateTimeFormat = "yyyyMMdd";

        /// <summary>
        /// Contains the filename to be used when logging
        /// </summary>
        private string mFilename = "";

        #endregion

        #region Internal Properties
        /// <summary>
        /// Sets/Gets the relative or absolute logging path 
        /// </summary>
        internal string LogPath
        {
            get { return mLogPath; }
            set { mLogPath = value; }
        }

        /// <summary>
        /// Gets/Sets the format of the datetime information appended to the filename.
        /// This will also decide how long a filename will be written to before creating a new file.
        /// </summary>
        /// <remarks>
        /// If the value represents an invalid datetime formatstring (or it is not configured),
        /// it will be set to the default value (yyyyMMdd).
        /// This will create a file named MyApplication19720121.log.
        /// </remarks>
        internal string DateTimeFormat
        {
            get { return mDateTimeFormat; }
            set
            {
                try
                {
                    string dateTime = DateTime.Now.ToString(value);
                }
                catch (FormatException)
                {
                    mDateTimeFormat = "yyyyMMdd";
                }
            }
        }

        /// <summary>
        /// Gets or sets the filename to be used.
        /// </summary>
        /// <remarks>
        /// If this value is not supplied in the application configuration file, the name of the 
        /// file will be <see cref="LogItem.AssemblyName"/>.log
        /// </remarks>
        internal string Filename
        {
            get
            {
                return mFilename;
            }
            set
            {
                mFilename = value;
            }
        }

        #endregion

        #region Private Methods
        
        /// <summary>
        /// Gets the log file name. The log directory will be created if it does not exists
        /// </summary>
        /// <returns>Absolute path inluding the log filename</returns>
        /// <param name="assemblyName">The name of the entering assembly</param>
        private string GetFullFileName(string assemblyName)
        {
            StringBuilder sb = new StringBuilder();
            string path = GetLogDirectory();
            sb.Append(path);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            sb.Append(mFilename.Length == 0 ? assemblyName : mFilename);
            sb.Append(DateTime.Now.ToString(mDateTimeFormat));
            sb.Append(".log");
            return sb.ToString();
        }

        /// <summary>
        /// Absolute path to the logging directory
        /// </summary>
        /// <returns>Absolute path including a trailing backspace</returns>
        private string GetLogDirectory()
        {
            StringBuilder sb = new StringBuilder();
            //Check to see if the logpath is relative
            if (IsPathRelative())
            {
                sb.Append(GetAbsoluteCodebasePath());
            }
            sb.Append(mLogPath);
            sb.Append(Path.DirectorySeparatorChar);
            return sb.ToString();
        }

        /// <summary>
        /// Checks to see if the configured logpath is releative or absolute
        /// </summary>
        /// <returns>True if the path is relative, otherwise false</returns>
        private bool IsPathRelative()
        {
            Regex regex = new Regex(@"\w:\\");
            return !regex.Match(mLogPath).Success;

        }

        /// <summary>
        /// Returns the location of the excuting assembly
        /// </summary>
        /// <returns>The local operation system representation of the executing assembly location</returns>
        private string GetAbsoluteCodebasePath()
        {
            Uri uri = new Uri(Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase));
            return uri.LocalPath;
        }

        #endregion

        #region Overridden Methods
        /// <summary>
        /// Logs the <see cref="LogItem"/> to a text file
        /// </summary>
        /// <param name="logItem">The <see cref="LogItem"/> to sink</param>
        internal override void WriteLog(LogItem logItem)
        {

            StreamWriter sw = File.AppendText(GetFullFileName(logItem.AssemblyName));
            sw.WriteLine(logItem.ToString());
            sw.Close();
            sw.Dispose();
        }
        #endregion

    }
}
