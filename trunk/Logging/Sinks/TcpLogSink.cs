using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;




namespace System.Core.Logging
{
    /// <summary>
    /// Writes a <see cref="LogItem"/> to a tcp socket network stream
    /// </summary>
    /// <example>
    /// The following example shows how to configure LogDotDot to use the <see cref="TcpLogSink"/> class.
    /// <code>
    /// <![CDATA[
    /// <LogDotNet>
    ///     <Sink name ="MyApplicationLogName" loglevel ="Debug" type ="LogDotNet.TcpLogSink, LogDotnet">
    ///         <port value ="23"/>
    ///     </Sink>
    /// </LogDotNet>
    /// ]]>
    /// </code>
    /// </example>
    /// <remarks>
    /// The default port number is 23(Telnet) witch allows a client computer to recieve log messages using a telnet client.
    /// </remarks>
    internal class TcpLogSink : LogSink
    {
        #region Member Variables

        /// <summary>
        /// The listeing server socket
        /// </summary>
        private Socket mServerSocket;
        
        /// <summary>
        /// A list of clients listening for log messages
        /// </summary>
        private List<TcpLogClient> mClients = new List<TcpLogClient>();

        /// <summary>
        /// The port number to listen on. Default is 23(Telnet)
        /// </summary>
        private int mPort = 23;

        #endregion

        #region Internal Properties
        /// <summary>
        /// Gets/Sets the port number to start listening on.
        /// </summary>
        internal int Port
        {
            get { return mPort; }
            set 
            {   
                mPort = value;
                if (mServerSocket == null)
                CreateServerSocket();
            }
        }


        #endregion

        #region Constructors

        /// <summary>
        /// Create a new instance of the TcpLogSink class
        /// </summary>
        public TcpLogSink()
        {
            
        }

        #endregion

        #region Overridden Methods
        /// <summary>
        /// Writes the logItem on a tcp connection
        /// </summary>
        /// <param name="logItem">The logItem to write</param>
        internal override void WriteLog(LogItem logItem)
        {
            
            lock (this)
            {
                

                List<TcpLogClient> deadClients = null;
                //Send the log message to all tcp clients
                foreach (TcpLogClient client in mClients)
                {
                    try
                    {
                       client.Send(logItem.ToString() + "\f");
                    }
                    catch
                    {
                        if (deadClients == null)
                        {
                            deadClients = new List<TcpLogClient>();
                        }
                        deadClients.Add(client);
                    }

                }

                //Remove dead clients if any
                if (deadClients != null)
                {
                    foreach (TcpLogClient client in deadClients)
                    {
                        RemoveTcpClient(client);
                    }
                }
            }
        }   

        /// <summary>
        /// Shuts down all connected clients and shuts down the listening server socket
        /// </summary>
        protected override void  Dispose(bool disposing)
        {
            if (!mDisposed)
            {
                if (disposing)
                {
                    foreach (TcpLogClient client in mClients)
                    {
                        client.Dispose();
                    }
                    mClients.Clear();
                    mServerSocket.Shutdown(SocketShutdown.Both);
                }
              }
            base.Dispose(disposing);
        }

        #endregion

        #region Private Methods
        
        /// <summary>
        /// Initializes the <see cref="mServerSocket"/> and starts listensing for client connections.
        /// </summary>
        private void CreateServerSocket()
        {
            mServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            mServerSocket.Bind(new IPEndPoint(IPAddress.Any, mPort));
            mServerSocket.Listen(0);
            mServerSocket.BeginAccept(new AsyncCallback(OnClientConnection), null);
        }

        /// <summary>
        /// Called by the <seealso cref="mServerSocket"/> when a new client connects.
        /// </summary>
        /// <param name="asyncResult"></param>
        private void OnClientConnection(IAsyncResult asyncResult)
        {
            
            //Accept the incoming connection 
            Socket clientSocket =  mServerSocket.EndAccept(asyncResult);
            
            //Create a new TcplogClient
            TcpLogClient client = new TcpLogClient(clientSocket);
            
            //Send a welcome message to the client 
            client.Send(CreateWelcomeMessage(clientSocket));
            
            //Add the new client to the list of listening clients
            AddTcpClient(client);

            //Begin accepting new conections
            mServerSocket.BeginAccept(new AsyncCallback(OnClientConnection), null);
        }

        /// <summary>
        /// Creates a welcome message for connected clients
        /// </summary>
        /// <param name="client">The client socket connected to the server</param>
        /// <returns>A welcome message to send to the client</returns>
        private string CreateWelcomeMessage(Socket client)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.Append(client.RemoteEndPoint.ToString());
            sb.Append(" is now connected to the logging service at ");
            sb.AppendLine(Environment.MachineName);
            return sb.ToString() + "\f";
        }
        
        /// <summary>
        /// Removes a client from the list of connected clients
        /// </summary>
        /// <param name="client">The client socket to remove</param>
        private void RemoveTcpClient(TcpLogClient client)
        {
            lock (this)
            {
                client.Dispose();
                mClients.Remove(client);
            }
        }

        /// <summary>
        /// Adds a new client to the the list of connected clients
        /// </summary>
        /// <param name="client">The client socket to add</param>
        private void AddTcpClient(TcpLogClient client)
        {
            lock (this)
            {
                mClients.Add(client);
            }
        }

        #endregion

        #region Nested Classes
        /// <summary>
        /// Represents a client listening for log messages
        /// </summary>
        private class TcpLogClient : IDisposable
        {

            #region Member Variables
            
            /// <summary>
            /// The network stream that will recieve log messages
            /// </summary>
            StreamWriter mNetWorkStream;

            /// <summary>
            /// The client socket
            /// </summary>
            Socket mClientSocket;

            /// <summary>
            /// Used by the dispose pattern
            /// </summary>
            bool mDisposed;

            #endregion

            #region Constructors
            
            /// <summary>
            /// Creates a new instance of the <see cref="TcpLogClient"/> class
            /// </summary>
            /// <param name="clientSocket"></param>
            internal  TcpLogClient(Socket clientSocket)
            {
                mClientSocket = clientSocket;
                mNetWorkStream = new StreamWriter(new NetworkStream(mClientSocket));
            }

            #endregion

            #region Internal Methods
            
            /// <summary>
            /// Sends a message to the network stream
            /// </summary>
            /// <param name="message">The message to send</param>
            internal void Send(string message)
            {
                mNetWorkStream.Write(message);
                mNetWorkStream.Flush();
            }

            #endregion
             
            #region IDisposable Members

            private void Dispose(bool disposing)
            {
                if (!this.mDisposed)
                {
                    if (disposing)
                    {
                        mNetWorkStream.Close();
                        mNetWorkStream.Dispose();
                        mClientSocket.Shutdown(SocketShutdown.Both);
                    }
                }
                mDisposed = true;         
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            #endregion
        }

        #endregion
    }
}
