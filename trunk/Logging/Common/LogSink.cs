using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Threading;

namespace System.Common.Logging
{
	/// <summary>
	/// Base class for implementing log sinks 
	/// </summary>
    public abstract class LogSink : IDisposable
	{
     
        #region Member Variables
        
        /// <summary>
        /// Thread safe queue that contains logitems te logged
        /// </summary>
        private Queue mLogQueue = Queue.Synchronized(new Queue());
        
        /// <summary>
        /// The thread that this  
        /// </summary>
        private Thread mSinkThread;
        
        /// <summary>
        /// Controls the thread
        /// </summary>
        private bool mIsRunning = true;
        
        /// <summary>
        /// Used by the dispose pattern
        /// </summary>
        protected bool mDisposed;

        /// <summary>
        /// The name of the log
        /// </summary>
        private string mName;
        
        /// <summary>
        /// The log level threshold 
        /// </summary>
        private LogLevel mLogLevel;


        #endregion  

        #region Constructors
        /// <summary>
        /// Creates a new instance of classes derived from <see cref="LogSink"/> and 
        /// starts listening for an incoming <see cref="LogItem"/>
        /// </summary>
        internal LogSink()
        {
            mSinkThread = new Thread(new ThreadStart(StartSink));
            mSinkThread.Name = this.GetType().Name;
            mSinkThread.IsBackground = true;
        }

        #endregion

        #region Internal Properties
        /// <summary>
        /// Sets or gets the log level threshold for this sink.
        /// </summary>
        internal LogLevel LogLevel
        {
            get { return mLogLevel; }
            set { mLogLevel = value; }
        }
	
        /// <summary>
        /// Sets or gets the unique name if of the sink
        /// </summary>
        internal string Name
        {
            get { return mName; }
            set { mName = value; }
        }

        #endregion

        #region Internal Methods
       

        /// <summary>
        /// Starts the thread for this sink
        /// </summary>
        internal void Start()
        {
            mSinkThread.Start();            
        }

        /// <summary>
        /// Sinks the <paramref name="logItem"/>
        /// </summary>
        /// <param name="logItem">The <see cref="LogItem"/> to sink</param>
        internal void Sink(LogItem logItem)
        {
            //Pur the logitem on the queue
            mLogQueue.Enqueue(logItem);
            //Notify  the thread to start dequeing the logqueue
            try
            {
                mSinkThread.Interrupt();
            }
            catch (ThreadInterruptedException) { }               
        }

        #endregion

        #region Public Virtual Methods
        /// <summary>
        /// Default implementation. Overridden in derived classes
        /// Writes the logitem to its destination.
        /// </summary>
        /// <param name="logItem">The <see cref="LogItem"/> to write</param>
        internal virtual void WriteLog(LogItem logItem)
        {
            Console.WriteLine(logItem.ToString());
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Entry methods for the thread that this sink is running on
        /// </summary>
        private void StartSink()
        {
            while (mIsRunning)
            {
               
                try
                {
                //Sink all logItems in the queue
                DequeueLog();
                //Put the thread to sleep until new LogItems arrive
                
                
                    Thread.Sleep(Timeout.Infinite);
                }
                catch (ThreadInterruptedException) { }               
            }
        }

        
        /// <summary>
        /// Dequeues all queued Logitems and writes them to their destinations
        /// </summary>
        private void DequeueLog()
        {
            while (mLogQueue.Count > 0)
            {
                try
                {
                    WriteLog((LogItem)mLogQueue.Dequeue());
                }
                catch(Exception)
                {

                }
            }
        }

        #endregion

        #region IDisposable Members


        /// <summary>
        /// Releases all resources associated with this <see cref="LogSink"/>
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        /// <summary>
        /// Releases all resources associated with this <see cref="LogSink"/>
        /// </summary>
        /// <param name="disposing">Set to true if called by caller, otherwise called by the finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!mDisposed)
            {
                if (disposing)
                {
                    mIsRunning = false;
                    mLogQueue.Clear();
                }
            }
            mDisposed = true;
        }

        /// <summary>
        /// Finalizer used in the Dispose pattern
        /// </summary>
        ~LogSink()
        {
            Dispose(false);
        }

        #endregion
    }
}
