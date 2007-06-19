using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace System.Core.Logging
{
    /// <summary>
    /// Writes a <see cref="LogItem"/> to a database table
    /// </summary>
    /// <example>
    /// The following example shows how to configure the <see cref="DatabaseLogSink"/> class
    /// <code>
    /// <![CDATA[
    /// <LogDotNet>
    ///     <Sink name ="MyApplicationLogName" loglevel ="Debug" type ="LogDotNet.DatabaseLogSink, LogDotnet">
    ///         <ConnectionString value ="Persist Security Info=False;Trusted_Connection=true;database=MyDatabase;server=MyServer"/>
    ///         <Tablename value ="MyLogTable"/>
    ///     </Sink>
    /// </LogDotNet>
    /// ]]>
    /// </code>
    /// The code to create a fullblown logging table looks like this
    /// <code>
    /// <![CDATA[
    /// CREATE TABLE MyLogTable
	/// (
	///     ApplicationName, VARCHAR(255),
	///     LoggerName, VARCHAR(255),
	///     Severity, VARCHAR(10),
	///     Message, VARCHAR(500),
	///     FullMessage, VARCHAR(2000),
	///     Exception, VARCHAR(2000),
	///     LogDate, DATETIME
	/// )
    /// ]]>
    /// </code>
    /// You don't need to supply all the columns if you don't have to.
    /// LogDotNet will only set the column values if one or more of the following columns are present in the logging table
    /// <list type="table">
    /// <para></para>
    /// <listheader><b>This table shows possible columns in the logging table and where it gets its values from.</b></listheader>
    /// <item><term>ApplicationName</term><description><see cref="LogItem.AssemblyName"/></description></item>
    /// <item><term>LoggerName</term><description><see cref="LogSink.Name"/></description></item>
    /// <item><term>Severity</term><description><see cref="LogItem.LogLevel"/></description></item>
    /// <item><term>Message</term><description><see cref="LogItem.Message"/></description></item>
    /// <item><term>FullMessage</term><description><see cref="LogItem.ToString()">LogItem.ToString()</see></description></item>
    /// <item><term>Exception</term><description><see cref="LogItem.Exception"/></description></item>
    /// <item><term>LogDate</term><description><see cref="LogItem.LogDate"/></description></item>
    ///</list>
    /// <para></para>
    /// If you need to change the connectionstring from code, do as follows
    /// <code>
    /// (Logger.Instance.GetSink("MyDatabaselogSink") as DatabaseLogSink).ConnectionString = "MyNewConnectionString";
    /// </code>
    /// </example>
    /// <remarks>
    /// Make sure that the log table columns are string based except for LogDate that has to be of the DateTime datatype.
    /// If the column size is to small, the string data will be truncated without further notice.
    /// </remarks>
    public class DatabaseLogSink : LogSink
    {
        /// <summary>
        /// The connectionstring used to open new connections
        /// </summary>
        private string mConnectionString = "";

        /// <summary>
        /// The name of the table to save the <see cref="LogItem"/>
        /// </summary>
        private string mTablename;

        /// <summary>
        /// Gets or sets the name of the logging table
        /// </summary>
        internal string Tablename
        {
            get { return mTablename; }
            set { mTablename = value; }
        }
	
        /// <summary>
        /// Gets or sets the connectionstring used to connect to the logging database
        /// </summary>
        public string ConnectionString
        {
            get 
            {
                lock (mConnectionString)
                {
                    return mConnectionString;
                }
            }
            set 
            {
                lock (mConnectionString)
                {
                    mConnectionString = value;
                }
            }
        }
	
        /// <summary>
        /// Writes the <see cref="LogItem"/> to a database table
        /// </summary>
        /// <param name="logItem">The <see cref="LogItem"/> to write</param>
        internal override void WriteLog(LogItem logItem)
        {
            //Create a connection to the destination database
            SqlConnection connection = new SqlConnection(mConnectionString);
            connection.Open();
            
            //Get the table to log to
            DataTable logDataTable = GetLogTable(connection);
            
            //Add a new row in the data table
            DataRowView dataRow = logDataTable.DefaultView.AddNew();
            try
            {
                if (logDataTable.Columns.IndexOf("ApplicationName") >= 0)
                {
                    dataRow["ApplicationName"] = logItem.AssemblyName.Length > logDataTable.Columns["ApplicationName"].MaxLength ? logItem.AssemblyName.Substring(1, logDataTable.Columns["ApplicationName"].MaxLength) : logItem.AssemblyName;
                }

                if (logDataTable.Columns.IndexOf("LoggerName") >= 0)
                {
                    dataRow["LoggerName"] = this.Name.Length > logDataTable.Columns["LoggerName"].MaxLength ? this.Name.Substring(1, logDataTable.Columns["LoggerName"].MaxLength) : this.Name;
                }

                if (logDataTable.Columns.IndexOf("Severity") >= 0)
                {
                    dataRow["Severity"] = logItem.LogLevel.ToString().Length > logDataTable.Columns["Severity"].MaxLength ? logItem.LogLevel.ToString().Substring(1, logDataTable.Columns["Severity"].MaxLength) : logItem.LogLevel.ToString();
                }

                if (logDataTable.Columns.IndexOf("Message") >= 0)
                {
                    dataRow["Message"] = logItem.Message.Length > logDataTable.Columns["Message"].MaxLength ? logItem.Message.Substring(1, logDataTable.Columns["Message"].MaxLength) : logItem.Message;
                }

                if (logDataTable.Columns.IndexOf("FullMessage") >= 0)
                {
                    string fullMessage = logItem.ToString();
                    dataRow["FullMessage"] = fullMessage.Length > logDataTable.Columns["FullMessage"].MaxLength ? fullMessage.Substring(1, logDataTable.Columns["FullMessage"].MaxLength) : fullMessage;
                }

                if (logDataTable.Columns.IndexOf("Exception") >= 0)
                {
                    string exception = logItem.Exception == null ? "None" : logItem.Exception.ToString();
                    dataRow["Exception"] = exception.Length > logDataTable.Columns["Exception"].MaxLength ? exception.Substring(1, logDataTable.Columns["Exception"].MaxLength) : exception;
                }

                if (logDataTable.Columns.IndexOf("LogDate") >= 0)
                {
                    dataRow["LogDate"] = logItem.LogDate;
                }

                dataRow.EndEdit();

                //Save the new row
                SaveTable(connection, logDataTable);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                    connection.Dispose();
                }
                logDataTable.Dispose();
            }
        }

        /// <summary>
        /// Gets the DataTable that will be used to add the new log item
        /// </summary>
        /// <param name="connection">The connection used to access the database</param>
        /// <returns>A datatable containing the column in the logging table</returns>
        private DataTable GetLogTable(SqlConnection connection)
        {
            SqlCommand command = new SqlCommand("SELECT * FROM " + mTablename + " WHERE 1 = 2", connection);
            SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
            try
            {
                DataTable logDataTable = new DataTable();
                dataAdapter.Fill(logDataTable);
                dataAdapter.FillSchema(logDataTable, SchemaType.Source);
                return logDataTable;
            }
            finally
            {
                command.Dispose();
                dataAdapter.Dispose();
            }
        }

        /// <summary>
        /// Saves the DataTable(logtable) to the database
        /// </summary>
        /// <param name="connection">The connection used to access the database</param>
        /// <param name="dataTable">The DataTable to save</param>
        private void SaveTable(SqlConnection connection,DataTable dataTable)
        {
            
            SqlDataAdapter dataAdapter = new SqlDataAdapter("SELECT * FROM " + mTablename + " WHERE 1 = 2", connection);

            SqlCommandBuilder commandBuilder = new SqlCommandBuilder();
            try
            {
                commandBuilder.DataAdapter = dataAdapter;
                dataAdapter.Update(dataTable);
            }
            finally
            {
                dataAdapter.Dispose();
                commandBuilder.Dispose();
            }    
        }
 


    }
}
