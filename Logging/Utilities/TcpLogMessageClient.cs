using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Sockets;
namespace System.Core.Logging
{
    /// <summary>
    /// Provides a simple approach towards reading log messages from a <see cref="TcpLogSink"/>.
    /// </summary>
    /// <example>
    /// The following example shows how to connect to a remove machine running LogDotNet with a <see cref="TcpLogSink"/>
    /// <code>
    /// <![CDATA[
    /// private delegate void SetTextDelegate(string text);
    ///
    /// private void ConnectButton_Click(object sender, EventArgs e)
    /// {
    ///     //Create a new instance of the TcpLogMessageClient class
    ///     TcpLogMessageClient logClient = new TcpLogMessageClient();
    ///     
    ///     //Connect to the "remote machine"
    ///     logClient.Connect("127.0.0.1", 23);
    ///     
    ///     //Set up an event handler for the MessageReceived event
    ///     logClient.MessageReceived += new TcpLogMessageClientEventHandler(logClient_MessageReceived);
    /// }
    /// 
    /// private void logClient_MessageReceived(object sender, TcpLogMessageClientEventArgs e)
    /// {
    ///     //This event is fired from another thread and we cannot access the listbox directly
    ///     //
    ///     SetText(e.Message);
    /// }
    /// 
    /// private void SetText(string text)
    /// {
    ///     //Is this another thread?
    ///     if (logListBox.InvokeRequired)
    ///         //Yes it is. Invoke SetText through the SetTextDelegate
    ///         logListBox.Invoke(new SetTextDelegate(SetText), new object[] { text });
    ///     else
    ///     {
    ///         //SetText is called from the same thread that created it and it is ok to access the listbox
    ///         logListBox.Items.Add(text);
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// </example>
    public class TcpLogMessageClient
    {

        private Socket mClient;

        #region Public Events
        /// <summary>
        /// This event is fired when a new message is received from the server
        /// </summary>
        public event TcpLogMessageClientEventHandler MessageReceived;

        #endregion

        #region Public Methods

        
        /// <summary>
        /// Connects to the machine running LogDotNet
        /// </summary>
        /// <param name="host">The hostname or IP adress</param>
        /// <remarks>
        /// If the <paramref name="host"/> is omitted, it will default to localhost on port 23 (default telnet)
        /// If the host is supplied without a port number, it will also default to port 23.
        /// </remarks>
        public void Connect(string host)
        {
            try
            {
                int port = 23;

                if (host.Length == 0)
                    Connect("127.0.0.1", port);
                else
                {
                    if (host.Contains(":"))
                        int.TryParse(host.Substring(host.LastIndexOf(":") + 1), out port);
                    Connect(host.Substring(0, host.LastIndexOf(":")), port);
                }
            }
            catch
            {
                OnMessageReceived(string.Format("Failed to connect to host {0}", host));
            }
        }
        
        
        
        /// <summary>
        /// Connects to a machine running LogDotnet
        /// </summary>
        /// <param name="host">The hostname or IP address</param>
        /// <param name="portNumber">The port number on the remote machine</param>
        public void Connect(string host, int portNumber)
        {
            try
            {
                //Logger.Instance.Info(string.Format("Host {0} has connected to the logging service on port {1}",host,portNumber));
                IPEndPoint ipEndPoint;
                
                
                IPHostEntry ipHostEntry = Dns.GetHostEntry(host);
                
                
                //Now use the first address of type InterNetwork
                foreach (IPAddress ipAddress in ipHostEntry.AddressList)
                {
                    if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                    {
                        ipEndPoint = new IPEndPoint(ipAddress, portNumber);
                        mClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        mClient.BeginConnect(ipEndPoint, new AsyncCallback(OnConnect), null);
                        break;
                    }
                }
                
            }
            catch (Exception ex)
            {
                OnMessageReceived(ex.Message);
            }
        }

        /// <summary>
        /// Disconnects from the server running LogDotnet
        /// </summary>
        public void Disconnect()
        {
            if (mClient != null)
            {
                try
                {
                    if (mClient.Connected)
                        mClient.Shutdown(SocketShutdown.Both);
                    mClient.BeginDisconnect(false, new AsyncCallback(OnDisconnect), null);
                }
                catch (Exception ex)
                {
                    OnMessageReceived(ex.Message);
                }
            }
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// This method is called when trying to connect to the server 
        /// </summary>
        /// <param name="asyncResult">The status of the asynchronous operation</param>
        private void OnConnect(IAsyncResult asyncResult)
        {
            try
            {
                
                mClient.EndConnect(asyncResult);

                StateObject state = new StateObject();
                
                mClient.BeginReceive(state.Buffer, 0, state.BufferSize, 0, new AsyncCallback(OnReceive), state);
            }
            catch (Exception ex)
            {
                OnMessageReceived(ex.Message);
            }
        }
        
        /// <summary>
        /// This method is called to check if there is something to read from the server 
        /// </summary>
        /// <param name="asyncResult">The status of the asynchronous operation</param>
        private void OnReceive(IAsyncResult asyncResult)
        {

            StateObject state = (StateObject)asyncResult.AsyncState;

            try
            {
                //Read data from the server socket 
                int bytesRead = mClient.EndReceive(asyncResult);


                //Check to see if anything has been read
                if (bytesRead > 0)
                {
                    Console.WriteLine(Encoding.ASCII.GetString(state.Buffer, 0, bytesRead));
                    //Append the received data so far 
                    state.StringBuffer.Append(Encoding.ASCII.GetString(state.Buffer, 0, bytesRead));

                    //Check to see if the received string contains the form feed delimiter
                    if (state.StringBuffer.ToString().Contains("\f"))
                    {
                        SplitMessage(state);
                    }

                    //Continue getting data from the server 
                    mClient.BeginReceive(state.Buffer, 0, state.BufferSize, 0, new AsyncCallback(OnReceive), state);
                }
            }
            catch (Exception ex)
            {
                OnMessageReceived(ex.Message);
            }
        }

        /// <summary>
        /// This method is called when trying to disconnect from the server
        /// </summary>
        /// <param name="asyncResult">The status of the asynchronous operation</param>
        private void OnDisconnect(IAsyncResult asyncResult)
        {
            try
            {
                mClient.EndDisconnect(asyncResult);
            }
            catch (Exception ex)
            {
                OnMessageReceived(ex.Message);
            }   
        }

        /// <summary>
        /// Splits the raw stream from the server into messages to be sent back through the <see cref="MessageReceived"/> event.
        /// </summary>
        /// <remarks>
        /// The message is delimited with "\f" (form feed) and this method splits the raw textdata from the server.
        /// </remarks>
        /// <param name="state">The <see cref="StateObject"/> that will be used here to get the received text from the server</param>
        private void SplitMessage(StateObject state)
        {
            //Get the string received so for
            string fullMessage = state.StringBuffer.ToString();

            //Regular expression to search for "\f"
            Regex regex = new Regex("\\f");
            int delimiterCount = regex.Matches(fullMessage).Count;

            //Split into separate messages based on "\f"
            string[] messages = fullMessage.Split(new string[] { "\f" }, StringSplitOptions.RemoveEmptyEntries);

            //Notify event listeners that there are new messages
            for (int i = 0; i < delimiterCount; i++)
            {
                OnMessageReceived(messages[i]);
            }
            

            //The fullMessage variable may contain incomplete 
            //message and we remove the complete messages from the state object's
            //StringBuffer property
            state.StringBuffer.Length = 0;
            int lastDelimiterindex = fullMessage.LastIndexOf("\f");
            if (lastDelimiterindex > 0)
            {
                state.StringBuffer.Append(fullMessage.Substring(lastDelimiterindex + 1));
            }
        }

        /// <summary>
        /// Triggers the <see cref="MessageReceived"/> event.
        /// </summary>
        /// <param name="message">The log message to send to the event listeners</param>
        private void OnMessageReceived(string message)
        {
            if (MessageReceived != null)
            {

                StringBuilder sb = new StringBuilder();
                sb.Append(message);
                sb.Replace("\r", "");
                sb.Replace("\n", "");
                sb.Replace("\f", "");
                MessageReceived(this, new TcpLogMessageClientEventArgs(sb.ToString()));

            }
        }

        #endregion

        #region Nested Classes

        /// <summary>
        /// Used in conjunction with the asynchronous communication with the server 
        /// </summary>
        private class StateObject
        {
            
            /// <summary>
            /// The number of bytes to read between each read operation
            /// </summary>
            private int mBufferSize = 255;

            /// <summary>
            /// Temporary storage for the bytes read from the network stream
            /// </summary>
            private byte[] mBuffer;

            /// <summary>
            /// The string representation of the incoming byte stream
            /// </summary>
            private StringBuilder mTextBuffer = new StringBuilder();

            /// <summary>
            /// Creates a new instane of the <see cref="StateObject"/> class.
            /// </summary>
            internal StateObject()
            {
                mBuffer = new byte[mBufferSize];
            }

           

            /// <summary>
            /// Gets the temporary storage for the bytes read from the network stream
            /// </summary>
            public byte[] Buffer
            {
                get { return mBuffer; }
            }

            /// <summary>
            /// Gets the number of bytes to read between each read operation
            /// </summary>
            public int BufferSize
            {
                get { return mBufferSize; }
            }

            /// <summary>
            /// string representation of the incoming byte stream.
            /// </summary>
            public StringBuilder StringBuffer
            {
                get { return mTextBuffer; }
            }
        }
        #endregion

    }
        

}
