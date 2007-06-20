using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Xml;
using System.Reflection;
using System.Threading;
using System.Text;
namespace System.Core.Database
{
    /// <summary>
    /// A connection class that provides a simplified way to execute sql statements and stored procedures 
    /// </summary>
    public class Connection : IDisposable
    {
        #region Public Events
        
        /// <summary>
        /// Occurs every time that the number of rows specified by the batchsize parameter when using bulk copy
        /// </summary>
        public event SqlRowsCopiedEventHandler RowsCopied;

        #endregion
        
        #region Instance Member Variables




        /// <summary>
        /// Reflects the type of command to execute
        /// </summary>
        internal enum ExecutionType { ExecuteNonQuery, ExecuteScalar, ExecuteDataTable, ExecuteDataTableWithSchema, ExecuteReader, ExecuteXmlReader };

        /// <summary>
        /// The SqlConnection object that is used to connect to the database
        /// </summary>
        private SqlConnection mSqlConnection;

        /// <summary>
        /// The Isolationlevel to be used with new transactions
        /// </summary>
        private IsolationLevel mIsolationLevel = IsolationLevel.ReadCommitted;

        private int mCommandTimeout = 30;

        /// <summary>
        /// The SqlTransaction object that is used to handle transactions on this connection
        /// </summary>
        private SqlTransaction mSqlTransaction;

        /// <summary>
        /// Used to get the embedded resources in this assembly 
        /// </summary>
        private static ResourceManager mResourceManager = new ResourceManager("System.Core.Database.Resources.ErrorCodes", Assembly.GetExecutingAssembly());

        /// <summary>
        /// This collection contains all the stored procedures executed on this connection. 
        /// </summary>
        private StoredProcedureDictionary mStoredProcedureCollection;

        /// <summary>
        /// The connectionstring for this connection
        /// </summary>
        private string mConnectionString;

        /// <summary>
        /// Used by the dispose pattern 
        /// </summary>
        private bool mDisposed;

        #endregion

        #region Constructors
        /// <overloads>Initializes a new instance of the <see cref="DbDotNet.Connection"/> class</overloads>	
        /// <summary>
        /// Initializes a new instance of the <see cref="DbDotNet.Connection"/> class
        /// </summary>
        /// <remarks>
        /// Constructor is depenendent on the following entry in App.Config
        /// 
        /// <code>
        /// <![CDATA[
        /// <appSettings>
        ///		<add key="ConnectionString" value="data source=BERNHARD;initial catalog=BERNHARD_UTV;Integrated Security=SSPI"/> 
        ///	</appSettings>
        /// </code>
        /// ]]>    
        /// </remarks>
        public Connection()
        {
            Initialize();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="DbDotNet.Connection"/> class
        /// </summary>
        /// <param name="connectionString">The connection string used to create a connection to the Sql Server</param>
        public Connection(string connectionString)
        {
            mConnectionString = connectionString;
            Initialize();
        }
        #endregion

        #region Private


        /// <summary>
        /// Initializes a new connection
        /// </summary>
        private void Initialize()
        {
            try
            {
                mSqlConnection = new SqlConnection();
                mSqlConnection.ConnectionString = mConnectionString;
                mStoredProcedureCollection = new StoredProcedureDictionary(this);

            }
            catch (Exception ex)
            {
                throw new DatabaseException(GetResourceString("SYSTEM_ERROR_FAILED_TO_INITIALIZE", mConnectionString != null ? mConnectionString : "null"), ex);
            }
        }

        /// <summary>
        /// Creates a selectCommand based on the <see cref="System.Data.DataTable"/>
        /// </summary>
        /// <param name="datatable">The DataTable that will be the source of information
        /// in the process of creating a selectCommand</param>
        /// <returns>A <see cref="System.Data.SqlClient.SqlCommand"/> that contains a select statement representing the DataTable</returns>
        private SqlCommand CreateSelectCommand(DataTable datatable)
        {
            StringBuilder sb = new StringBuilder("SELECT ");
            foreach (DataColumn column in datatable.Columns)
            {
                sb.Append(column.ColumnName);
                sb.Append(", ");
            }
            sb.Remove(sb.Length - 2, 2);
            sb.Append(" FROM ");
            sb.Append(datatable.TableName);
            return CreateCommand(CommandType.Text, sb.ToString());
        }

        /// <summary>
        /// Returns the error description based on the given name
        /// </summary>
        /// <param name="name">The name of the error to get description</param>
        /// <returns>The description of the given error name</returns>
        private static string GetResourceString(string name)
        {
            string resourceValue;
            resourceValue = mResourceManager.GetString(name);
            Debug.Assert(resourceValue != null, "name not found");
            return resourceValue;
        }

        /// <summary>
        /// Gets the parameters for the <paramref name="command"/> from the parameters cache
        /// </summary>
        /// <param name="command">The <see cref="System.Data.SqlClient.SqlCommand"/> for witch to get the parameters</param>
        private static void GetParametersFromCache(SqlCommand command)
        {
            IDataParameter[] parameters = ParameterCache.GetCachedParameters(command);
            for (int i = 0, j = parameters.Length; i < j; i++)
            {
                command.Parameters.Add(parameters[i]);
            }
        }

        /// <summary>
        /// Event handler for the <see cref="System.Data.SqlClient.SqlBulkCopy.SqlRowsCopied"/> event.
        /// </summary>
        /// <param name="sender">The object raising the event</param>
        /// <param name="e">Event argument</param>
        private void sqlBulkCopy_SqlRowsCopied(object sender, SqlRowsCopiedEventArgs e)
        {
            if (RowsCopied != null)
                RowsCopied(this, e);
        }

        #endregion

        #region Public
        /// <summary>
        /// Provides a simple method to save a <see cref="System.Data.DataTable"/> back to the database.
        /// </summary>
        /// <remarks>
        ///  In order for this method to work, the DataTable should consist only of one source table and 
        ///  the primary key should be present amoung the table columns.
        /// </remarks>
        /// <param name="dataTable">The <see cref="System.Data.DataTable"/> to save.</param>
        public void Save(DataTable dataTable)
        {
            bool wrappedCommand = false;

            if (mSqlTransaction == null)
            {
                this.BeginTransaction();
                wrappedCommand = true;
            }


            SqlDataAdapter dataAdapter = new SqlDataAdapter();
            SqlCommandBuilder commandBuilder = new SqlCommandBuilder();
            try
            {
                dataAdapter.SelectCommand = CreateSelectCommand(dataTable);
                commandBuilder.DataAdapter = dataAdapter;
                dataAdapter.Update(dataTable);
                if (wrappedCommand == true)
                {
                    this.Commit();
                }
            }
            catch (Exception ex)
            {
                mSqlConnection.Close();
                if (mSqlTransaction != null)
                {
                    mSqlTransaction.Dispose();
                    mSqlTransaction = null;
                }
                throw new DatabaseException(GetResourceString("SYSTEM_ERROR_FAILED_TO_SAVE_TABLE"), ex);
            }
            finally
            {
                dataAdapter.Dispose();
                commandBuilder.Dispose();
            }
        }

        /// <summary>
        /// Bulk copies a <see cref="System.Data.DataTable"/> into a database table.
        /// </summary>
        /// <param name="dataTable">Data that is to be bulk copied</param>
        /// <remarks>
        /// The <see cref="System.Data.DataTable.TableName"/> property will be used to identify the destination database table.
        /// <para/>
        /// Data will be copied using the <see cref="System.Data.SqlClient.SqlBulkCopyOptions.Default"/> setting.
        /// <para/>
        /// All the rows from the <paramref name="dataTable"/> will be copied in one batch.
        /// </remarks>
        /// <example>The following example shows how to copy rows from the [Order Details] table to the [Order Details Copy] table using the Northwind database.
        ///<code>
        /// <![CDATA[
        /// //Create a new connection 
        /// Connection connection = new Connection();
        /// //Get all the rows from the source table
        /// DataTable dataTable = connection.ExecuteDataTable("SELECT * FROM [Order Details]");
        /// //Set the name of the destination table
        /// dataTable.TableName = "[Order Details Copy]";
        /// //Perform the bulk copy
        /// connection.BulkCopy(dataTable);
        /// ]]>
        /// </code> 
        ///</example>
        public void BulkCopy(DataTable dataTable)
        {
            BulkCopy(dataTable, 0,30, SqlBulkCopyOptions.Default, dataTable.TableName);
        }

        /// <summary>
        /// Bulk copies a <see cref="System.Data.DataTable"/> into a database table.
        /// </summary>
        /// <param name="dataTable">Data that is to be bulk loaded</param>
        /// <param name="batchSize">The number of rows in each batch. Setting this value to 0, will cause all rows to be copied in one batch.</param>
        /// <remarks>
        /// The <see cref="System.Data.DataTable.TableName"/> property will be used to identify the destination database table.
        /// <para/>
        /// Data will be copied using the <see cref="System.Data.SqlClient.SqlBulkCopyOptions.Default"/> setting.
        /// </remarks>
        public void BulkCopy(DataTable dataTable, int batchSize)
        {
            BulkCopy(dataTable, batchSize, 30,SqlBulkCopyOptions.Default, dataTable.TableName);
        }

        /// <summary>
        /// Bulk copies a <see cref="System.Data.DataTable"/> into a database table.
        /// </summary>
        /// <param name="dataTable">Data that is to be bulk loaded</param>
        /// <param name="batchSize">The number of rows in each batch. Setting this value to 0, will cause all rows to be copied in one batch.</param>
        /// <param name="timeout">Number of seconds for the operation to complete before it times out</param>
        /// <remarks>
        /// The <see cref="System.Data.DataTable.TableName"/> property will be used to identify the destination database table.
        /// </remarks>
        public void BulkCopy(DataTable dataTable, int batchSize,int timeout)
        {
            BulkCopy(dataTable, batchSize, timeout, SqlBulkCopyOptions.Default, dataTable.TableName);
        }

        /// <summary>
        /// Bulk copies a <see cref="System.Data.DataTable"/> into a database table.
        /// </summary>
        /// <param name="dataTable">Data that is to be bulk loaded</param>
        /// <param name="batchSize">The number of rows in each batch. Setting this value to 0, will cause all rows to be copied in one batch.</param>
        /// <param name="timeout">Number of seconds for the operation to complete before it times out</param>
        /// <param name="copyOptions">Bitwise flag that specifies one or more options to be used during bulk copy</param>
        /// <remarks>
        /// The <see cref="System.Data.DataTable.TableName"/> property will be used to identify the destination database table.
        /// </remarks>
        public void BulkCopy(DataTable dataTable, int batchSize,int timeout, SqlBulkCopyOptions copyOptions)
        {
            BulkCopy(dataTable, batchSize, timeout ,copyOptions, dataTable.TableName);
        }

        /// <summary>
        /// Bulk copies a <see cref="System.Data.DataTable"/> into a database table.
        /// </summary>
        /// <param name="dataTable">Data that is to be bulk loaded</param>
        /// <param name="batchSize">The number of rows in each batch. Setting this value to 0, will cause all rows to be copied in one batch.</param>
        /// <param name="timeout">Number of seconds for the operation to complete before it times out</param>
        /// <param name="copyOptions">Bitwise flag that specifies one or more options to be used during bulk copy</param>
        /// <param name="destinationTableName">The name of the destination database table</param>
        public void BulkCopy(DataTable dataTable, int batchSize, int timeout,SqlBulkCopyOptions copyOptions,string destinationTableName)
        {
            bool wrappedCommand = false;

            if (mSqlTransaction == null)
            {
                this.BeginTransaction();
                wrappedCommand = true;
            }

            //Remove the UseInternalTransaction flag since this is running within a transaction.
            copyOptions &= ~SqlBulkCopyOptions.UseInternalTransaction;
   
            try
            {
                SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(mSqlConnection, copyOptions, mSqlTransaction);
                sqlBulkCopy.DestinationTableName = destinationTableName;
                sqlBulkCopy.BatchSize = batchSize;
                sqlBulkCopy.NotifyAfter = batchSize;
                sqlBulkCopy.BulkCopyTimeout = timeout;
                sqlBulkCopy.SqlRowsCopied += new SqlRowsCopiedEventHandler(sqlBulkCopy_SqlRowsCopied);
                sqlBulkCopy.WriteToServer(dataTable);
                
                if (wrappedCommand == true)
                {
                    this.Commit();
                }
                
                sqlBulkCopy.Close();
            }
            catch (Exception ex)
            {
                mSqlConnection.Close();
                if (mSqlTransaction != null)
                {
                    mSqlTransaction.Dispose();
                    mSqlTransaction = null;
                }
                throw new DatabaseException(GetResourceString("SYSTEM_ERROR_FAILED_TO_BULKCOPY", destinationTableName), ex);
            }

        }

        /// <summary>
        /// Starts a new transaction on this connection.
        /// </summary>
        public void BeginTransaction()
        {
            if (mSqlTransaction == null)
            {
                try
                {
                    mSqlConnection.Open();
                    mSqlTransaction = mSqlConnection.BeginTransaction(mIsolationLevel);
                }
                catch (Exception ex)
                {
                    throw new DatabaseException(GetResourceString("SYSTEM_ERROR_FAILED_TO_BEGIN_TRANSACTION"), ex);
                }
            }
            else
            {
                throw new DatabaseException(GetResourceString("USER_ERROR_FAILED_TO_BEGIN_TRANSACTION"));
            }
        }

        /// <summary>
        /// Starts a new transaction on this connection with the given isolationlevel
        /// </summary>
        /// <param name="isolationLevel"></param>
        public void BeginTransaction(IsolationLevel isolationLevel)
        {
            mIsolationLevel = isolationLevel;
            BeginTransaction();
        }


        /// <summary>
        /// Use this method to test if a connection to the server can be made.
        /// </summary>
        /// <exception cref="DatabaseException"/>
        public void TestConnection()
        {
            SqlConnection connection = new SqlConnection(mConnectionString);
            
            try
            {
                connection.Open();
            }
            catch(Exception ex)
            {
                throw new DatabaseException(string.Format(GetResourceString("SYSTEM_ERROR_FAILED_TO_OPEN_CONNECTION"),mConnectionString), ex);
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }
        }



        /// <summary>
        /// Gets or sets the <see cref="System.Data.IsolationLevel"/> level for new transactions
        /// The default value is <see cref="System.Data.IsolationLevel.ReadCommitted"/>
        /// </summary>
        public IsolationLevel IsolationLevel
        {
            get
            {
                return mIsolationLevel;
            }
            set
            {
                mIsolationLevel = value;
            }
        }

        /// <summary>
        /// Gets or sets the wait time in seconds before terminating the attempt to execute a command and generating an error.
        /// </summary>
        public int CommandTimeout
        {
            get
            {
                return mCommandTimeout;
            }
            set
            {
                mCommandTimeout = value;
            }
        }
        
        /// <summary>
        /// Returns the connection string that was used to create this connection
        /// </summary>
        public string ConnectionString
        {
            get
            {
                return mSqlConnection != null ? mSqlConnection.ConnectionString : "";
            }
        }

        /// <summary>
        /// Creates a connectionstring using for use with Windows Authentication.
        /// </summary>
        /// <param name="server">The name or address of the server to connect to.</param>
        /// <param name="database">The database on the server to connecto to.</param>
        /// <returns>A connectionstring that can be used to connect to a SQL server database.</returns>
        public static string CreateConnectionString(string server,string database)
        {
            return string.Format("Data Source={0};Initial Catalog={1};Integrated Security=True", server, database);    
        }

        /// <summary>
        /// Creates a connectionstring using for use with SQL Server Authentication.
        /// </summary>
        /// <param name="server">The name or address of the server to connect to.</param>
        /// <param name="database">The database on the server to connecto to.</param>        
        /// <param name="username">The username used to connect to the server.</param>
        /// <param name="password">The password used to connection to the server. </param>
        /// <returns></returns>
        public static string CreateConnectionString(string server, string database,string username,string password)
        {
            return string.Format("Data Source={0};Initial Catalog={1};Persist Security Info=True;User ID={2};Password={3}",server,database,username,password);
        }

        /// <summary>
        /// Issues a rollback on this connection
        /// </summary>
        public void Rollback()
        {
            if (mSqlTransaction != null)
            {
                try
                {
                    mSqlTransaction.Rollback();

                }
                catch (Exception ex)
                {
                    throw new DatabaseException(GetResourceString("SYSTEM_ERROR_FAILED_TO_ROLLBACK_TRANSACTION"), ex);
                }

                finally
                {
                    mSqlTransaction.Dispose();
                    mSqlTransaction = null;
                    mSqlConnection.Close();
                }
            }
            else
            {
                throw new DatabaseException(GetResourceString("USER_ERROR_FAILED_TO_ROLLBACK_TRANSACTION"));
            }
        }

        /// <summary>
        /// Issues a commit on this connection
        /// </summary>
        public void Commit()
        {
            if (mSqlTransaction != null)
            {
                try
                {
                    mSqlTransaction.Commit();
                }

                catch (Exception ex)
                {
                    throw new DatabaseException(GetResourceString("SYSTEM_ERROR_FAILED_TO_COMMIT_TRANSACTION"), ex);
                }
                finally
                {
                    mSqlTransaction.Dispose();
                    mSqlTransaction = null;
                    mSqlConnection.Close();
                }
            }
            else
            {
                throw new DatabaseException(GetResourceString("USER_ERROR_FAILED_TO_COMMIT_TRANSACTION"));
            }
        }

        ///<summary>
        /// The <see cref="DbDotNet.StoredProcedure"/> collection used to execute stored procedures on this <see cref="DbDotNet.Connection"/>.
        /// </summary>
        public StoredProcedureDictionary StoredProcedures
        {
            get
            {
                return mStoredProcedureCollection;
            }
        }

        /// <summary>
        /// Executes the <paramref name="commandText"/> and returns a <see cref="System.Data.DataTable"/>.
        /// </summary>
        /// <param name="commandText">The SQL statement to be executed</param>
        /// <returns><see cref="System.Data.DataTable"/></returns>
        public DataTable ExecuteDataTable(string commandText)
        {
            SqlCommand command = CreateCommand(CommandType.Text, commandText);
            return (DataTable)ExecuteCommand(ExecutionType.ExecuteDataTable, command);
        }

        /// <summary>
        /// Executes the <paramref name="commandText"/> and returns a <see cref="System.Data.DataTable"/>.
        /// </summary>
        /// <param name="commandText">The SQL statement to be executed</param>
        /// <param name="fillSchema">Setting this to true, will include the schema in the DataTable</param>
        /// <returns><see cref="System.Data.DataTable"/></returns>
        public DataTable ExecuteDataTable(string commandText, bool fillSchema)
        {
            SqlCommand command = CreateCommand(CommandType.Text, commandText);
            return ExecuteDataTable(command, fillSchema);
        }

        /// <summary>
        /// Executes the <paramref name="command"/> and returns a <see cref="System.Data.DataTable"/>.
        /// </summary>
        /// <param name="command">The command to be executed</param>
        /// <param name="fillSchema">Setting this to true, will include the schema in the DataTable</param>
        /// <returns><see cref="System.Data.DataTable"/></returns>
        public DataTable ExecuteDataTable(SqlCommand command, bool fillSchema)
        {
            Connection.ExecutionType executionType = fillSchema == true ?
                Connection.ExecutionType.ExecuteDataTableWithSchema : Connection.ExecutionType.ExecuteDataTable;
            PrepareCommand(command);
            return (DataTable)ExecuteCommand(executionType, command);
        }


        /// <summary>
        /// Executes the <paramref name="commandText"/> and returns the number of rows affected
        /// </summary>
        /// <param name="commandText">The SQL statement to be executed</param>
        /// <returns><see cref="System.Int32"/></returns>
        public int ExecuteNonQuery(string commandText)
        {
            SqlCommand command = CreateCommand(CommandType.Text, commandText);
            return (int)ExecuteCommand(ExecutionType.ExecuteNonQuery, command);
        }

        /// <summary>
        /// Executes the <paramref name="command"/> and returns the number of rows affected
        /// </summary>
        /// <param name="command">The command to be executed</param>
        /// <returns><see cref="System.Int32"/></returns>
        public int ExecuteNonQuery(SqlCommand command)
        {
            PrepareCommand(command);
            return (int)ExecuteCommand(ExecutionType.ExecuteNonQuery, command);
        }


        /// <summary>
        /// Executes the <paramref name="commandText"/> and returns a <see cref="System.Data.SqlClient.SqlDataReader"/>.
        /// </summary>
        /// <param name="commandText">The SQL statement to be executed</param>
        /// <returns><see cref="System.Data.SqlClient.SqlDataReader"/></returns>
        public SqlDataReader ExecuteReader(string commandText)
        {
            SqlCommand command = CreateCommand(CommandType.Text, commandText);
            return (SqlDataReader)ExecuteCommand(ExecutionType.ExecuteReader, command);
        }

        /// <summary>
        /// Executes the <paramref name="commandText"/> and returns a <see cref="System.Xml.XmlReader"/>.
        /// </summary>
        /// <param name="commandText">The SQL statement to be executed</param>
        /// <returns><see cref="System.Xml.XmlReader"/></returns>
        public XmlReader ExecuteXmlReader(string commandText)
        {
            SqlCommand command = CreateCommand(CommandType.Text, commandText);
            return (XmlReader)ExecuteCommand(ExecutionType.ExecuteXmlReader, command);
        }

        /// <summary>
        /// Executes the <paramref name="commandText"/> and returns the first column on the first row.
        /// Extra columns and rows are ignored.
        /// </summary>
        /// <param name="commandText">The SQL statement to be executed</param>
        /// <returns><see cref="System.Object"/></returns>
        public object ExecuteScalar(string commandText)
        {
            SqlCommand command = CreateCommand(CommandType.Text, commandText);
            return ExecuteCommand(ExecutionType.ExecuteScalar, command);

        }

        /// <summary>
        /// Executes the <paramref name="command"/> and returns the first column on the first row.
        /// Extra columns and rows are ignored.
        /// </summary>
        /// <param name="command">The command to be executed</param>
        /// <returns><see cref="System.Object"/></returns>
        public object ExecuteScalar(SqlCommand command)
        {
            PrepareCommand(command);
            return ExecuteCommand(ExecutionType.ExecuteScalar, command);

        }
        #endregion

        #region Public Static Methods

        /// <summary>
        /// Selects a disctint set of rows based on the <paramref name="sourceColumn"/>
        /// </summary>
        /// <param name="sourceTable">Table containing source rows</param>
        /// <param name="sourceColumn">The name of the column that is to be used as the distinct argument</param>
        /// <returns>A distinct list of rows based on the <paramref name="sourceColumn"/></returns>
        public static DataTable SelectDistinct(DataTable sourceTable, string sourceColumn)
        {
            DataTable resultTable = null;
            try
            {
                resultTable = new DataTable();
                for (int i = 0; i < sourceTable.Columns.Count; i++)
                {
                    resultTable.Columns.Add(sourceTable.Columns[i].ColumnName, sourceTable.Columns[i].DataType);
                }

                Hashtable hashTable = new Hashtable();
                foreach (DataRow dataRow in sourceTable.Rows)
                {
                    if (!hashTable.ContainsKey(dataRow[sourceColumn]) && (dataRow[sourceColumn].ToString().Length > 0))
                    {
                        DataRow newResultRow = resultTable.NewRow();
                        hashTable.Add(dataRow[sourceColumn], null);

                        for (int i = 0; i < resultTable.Columns.Count; i++)
                        {
                            newResultRow[i] = dataRow[i];
                        }
                        resultTable.Rows.Add(newResultRow);
                    }
                }
                return resultTable;
            }
            catch(Exception ex)
            {
                throw new DatabaseException(GetResourceString("SYSTEM_ERROR_FAILED_TO_SELECTDISTINCT", resultTable.TableName), ex);
            }
           
        }

        /// <summary>
        /// Creates a new datatable based on the <paramref name="filterExpression"/>
        /// </summary>
        /// <param name="sourceTable">Table containing source rows</param>
        /// <param name="filterExpression">The filterexpression used to match the rows in the <paramref name="sourceTable"/></param>
        /// <returns>A new DataTable containg the result of the <paramref name="filterExpression"/></returns>
        public static DataTable Select(DataTable sourceTable, string filterExpression)
        {
            DataTable table = sourceTable.Clone();
            table.Clear();

            DataRow[] rows = sourceTable.Select(filterExpression);

            for (int i = 0; i < rows.Length; i++)
            {
                table.ImportRow(rows[i]);
            }

            return table;
        }

        #endregion



        #region Internal

        /// <summary>
        /// Creates a new <see cref="System.Data.SqlClient.SqlCommand"/>. 
        /// </summary>
        /// <param name="commandType">The <see cref="System.Data.CommandType"/> to create.</param>
        /// <param name="commandText">Stored procedure name or SQL statement to be executed.</param>
        /// <returns><see cref="System.Data.SqlClient.SqlCommand"/></returns>
        internal SqlCommand CreateCommand(CommandType commandType, string commandText)
        {
            try
            {
                SqlCommand command = new SqlCommand(commandText);
                command.CommandType = commandType;
                PrepareCommand(command);
                return command;
            }
            catch (Exception ex)
            {
                throw new DatabaseException(GetResourceString("SYSTEM_ERROR_FAILED_TO_EXECUTE_COMMAND", commandText), ex);
            }
        }

        internal void PrepareCommand(SqlCommand command)
        {
            command.CommandTimeout = mCommandTimeout;
            command.Connection = mSqlConnection;
            command.Transaction = mSqlTransaction;
        }


        /// <summary>
        /// Takes care of the actual execution of all commands
        /// </summary>
        /// <param name="executionType">The type execution to perform</param>
        /// <param name="command">The <see cref="System.Data.SqlClient.SqlCommand"/> to execute</param>
        /// <returns>The result of the command execution</returns>
        internal object ExecuteCommand(ExecutionType executionType, SqlCommand command)
        {
            bool wrappedCommand = false;
            object returnValue;
            try
            {
                if (mSqlTransaction == null)
                {
                    this.BeginTransaction();
                    wrappedCommand = true;
                }

                command.Transaction = mSqlTransaction;

                switch (executionType)
                {
                    case ExecutionType.ExecuteNonQuery:
                        {
                            returnValue = command.ExecuteNonQuery();
                            break;
                        }

                    case ExecutionType.ExecuteDataTable:
                    case ExecutionType.ExecuteDataTableWithSchema:
                        {
                            SqlDataAdapter da = new SqlDataAdapter(command);
                            DataTable dt = new DataTable();
                            dt.Locale = CultureInfo.InvariantCulture;
                            if (executionType == ExecutionType.ExecuteDataTableWithSchema)
                            {
                                da.FillSchema(dt, SchemaType.Source);
                            }
                            da.Fill(dt);
                            returnValue = dt;
                            break;
                        }

                    case ExecutionType.ExecuteReader:
                        {
                            returnValue = command.ExecuteReader();
                            break;
                        }

                    case ExecutionType.ExecuteScalar:
                        {
                            returnValue = command.ExecuteScalar();
                            break;
                        }

                    case ExecutionType.ExecuteXmlReader:
                        {
                            returnValue = command.ExecuteXmlReader();
                            break;
                        }

                    default:
                        {
                            return null;
                        }

                }
                if (wrappedCommand)
                {
                    this.Commit();
                }
            }
            catch (Exception ex)
            {

                mSqlConnection.Close();
                if (mSqlTransaction != null)
                {
                    mSqlTransaction.Dispose();
                    mSqlTransaction = null;
                }

                throw new DatabaseException(GetResourceString("SYSTEM_ERROR_FAILED_TO_EXECUTE_COMMAND", command.CommandText), ex);
            }


            return returnValue;

        }

        /// <summary>
        /// Returns the error description based on the given name
        /// </summary>
        /// <param name="name">The name of the error to get description</param>
        /// <param name="parameters">Used to fill out the placeholders in the error description</param>
        /// <returns>A <see cref="System.String"/> that contains the full errors message</returns>
        internal static string GetResourceString(string name, params string[] parameters)
        {
            string resourceValue;
            resourceValue = string.Format(CultureInfo.InvariantCulture, mResourceManager.GetString(name), parameters);            
            return resourceValue;
        }

        /// <summary>
        /// Gets the parameters for <paramref name="command"/>.
        /// </summary>
        /// <param name="command">The <see cref="System.Data.SqlClient.SqlCommand"/> for witch to get the parameters</param>
        internal void GetCommandParameters(SqlCommand command)
        {

            //First check to see if there is cached parameter for this command
            if (ParameterCache.IsParametersCached(command))
            {
                //get the parameters from cache
                GetParametersFromCache(command);
            }
            else
            {
                //create a new connection to derive the parameters
                try
                {
                    SqlConnection connection = new SqlConnection(mConnectionString);
                    command.Connection = connection;
                    command.Transaction = null;
                    connection.Open();

                    SqlCommandBuilder.DeriveParameters(command);

                    command.Connection.Close();
                    command.Connection = mSqlConnection;
                    command.Transaction = mSqlTransaction;    

                    //Cache the parameters for later use
                    ParameterCache.CacheParameters(command);
                }
                catch
                {
                }
            }
        }
        #endregion

        #region IDisposable Members


        /// <summary>
        /// Releases all resources related to this instance of the <see cref="DbDotNet.Connection"/> 
        /// If the conection is in a transaction, a rollback will be executed 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Takes care of the actual disposing of this object
        /// </summary>
        /// <param name="disposing">True if called by users of this class, otherwise it has been called by the <see cref="System.GC">Garbage Collector</see>.</param>
        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!mDisposed)
            {
                // If disposing equals true, dispose all managed 
                // and unmanaged resources.
                if (disposing)
                {
                    mSqlConnection.Close();
                    mSqlConnection.Dispose();
                }
            }
            mDisposed = true;
        }

        /// <summary>
        /// Finalizer used by the dispose patteren
        /// </summary>
        ~Connection()
        {
            Dispose(false);
        }
        #endregion
    }
}
