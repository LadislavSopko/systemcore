using System;
using System.Collections.Generic;
using System.Text;

namespace System.Common.Logging
{
        /// <summary>
        /// Describes the severity of the log message.
        /// </summary>
        internal enum LogLevel
		{
			/// <summary>
			/// Log a debug log message
			/// </summary>
            Debug,
            /// <summary>
            /// Log an information log message
            /// </summary>
			Info,
            /// <summary>
            /// Log a warning log message
            /// </summary>
			Warning,
            /// <summary>
            /// Log an error log message
            /// </summary>
			Error,
            /// <summary>
            /// Log a fatal error message
            /// </summary>
			Fatal
		}
}
