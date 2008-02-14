using System;
using System.Collections.Generic;
using System.Text;

namespace System.Common.Logging
{
    /// <summary>
    /// Represents the method that will handle the <see cref="TcpLogMessageClient.MessageReceived">MessageReceived</see> event. 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void TcpLogMessageClientEventHandler(object sender, TcpLogMessageClientEventArgs e);

    /// <summary>
    /// Provides additional information for the <see cref="TcpLogMessageClient.MessageReceived">MessageReceived</see> event.
    /// </summary>
    public class TcpLogMessageClientEventArgs : EventArgs
    {
        #region Private Variables
        
        /// <summary>
        /// The log message received from the server
        /// </summary>
        private string mMessage;

        #endregion

        #region Constructors
        
        /// <summary>
        /// Creates a new instance of the <see cref="TcpLogMessageClientEventArgs"/> class.
        /// </summary>
        /// <param name="message"></param>
        internal TcpLogMessageClientEventArgs(string message)
        {
            mMessage = message;
        }

        #endregion

        #region Public Properties
        
        /// <summary>
        /// Gets the log message from the server
        /// </summary>
        public string Message
        {
            get { return mMessage; }
        }

        #endregion
    }
}
