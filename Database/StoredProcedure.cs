using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Xml;
using System.Collections.Generic;
using System.Text;

namespace System.Core.Database
{
    /// <summary>
    /// This class is used to execute stored procedures against a Microsoft SQL Server.
    /// </summary>
    /// <example>The following example shows how to call a stored procedure using DbDotNet
    /// <code>
    /// Connection connection = new Connection();
    /// connection.StoredProcedures["MyStoredProcedure"].ExecuteNonQuery("MyStringParameterValue");
    /// </code>
    /// </example>
    public class StoredProcedure
    {

        #region Private Member Variables
        /// <summary>
        /// The <see cref="System.Data.SqlClient.SqlCommand"/> used to execute the stored procedure
        /// </summary>
        private SqlCommand mSqlCommand;
        
        /// <summary>
        /// The <see cref="DbDotNet.Connection"/> that is used to execute this stored procedure
        /// </summary>
        private Connection mConnection;

        /// <summary>
        /// Indicates if the parameters has been collected from the <see cref="DbDotNet.ParameterCache"/>
        /// </summary>
        private bool mHasDerivedParameters;

        /// <summary>
        /// Indicates if the <see cref="System.Data.DataTable"/> returned by <see cref="ExecuteDataTable()"/> should be populated with the source schema.
        /// </summary>
        private bool mFillSchema;

        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new instance of the <see cref="DbDotNet.StoredProcedure"/> class.
        /// </summary>
        /// <param name="connection">The <see cref="DbDotNet.Connection"/> that this procedure belongs to.</param>
        /// <param name="procedureName">The name of the stored procedure</param>
        internal StoredProcedure(Connection connection, string procedureName)
        {
            mConnection = connection;
            mSqlCommand = mConnection.CreateCommand(CommandType.StoredProcedure, procedureName);

        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Fills the parameter collection with the supplied values.
        /// </summary>
        /// <param name="parameters">A <see cref="System.Object"/> array containing zero of more parameters values</param>
        private void FillParameters(params object[] parameters)
        {
            //Get the parameters for this command
            GetCommandParameters();

            if (parameters != null)
            {
                //Fill the parameters
                int parameterCount = mSqlCommand.Parameters.Count - 1 < parameters.Length ?
                    mSqlCommand.Parameters.Count - 1 : parameters.Length;

                for (int i = 1; i <= parameterCount; i++)
                {
                    mSqlCommand.Parameters[i].Value = parameters[i - 1] == null ? DBNull.Value : parameters[i - 1];
                }
            }
        }

        /// <summary>
        /// Gets the parameters from the <see cref="DbDotNet.ParameterCache"/>.
        /// This will only run the first time the procedure is executed or the first time after <see cref="ResetParameters"/>.
        /// </summary>
        private void GetCommandParameters()
        {
            if (mHasDerivedParameters != true)
            {
                mConnection.GetCommandParameters(mSqlCommand);
                mHasDerivedParameters = true;
            }
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// Returns the <see cref="System.Data.SqlClient.SqlParameterCollection"/> for this <see cref="DbDotNet.StoredProcedure"/>.
        /// </summary>
        public SqlParameterCollection Parameters
        {
            get
            {
                return mSqlCommand.Parameters;
            }
        }

        /// <summary>
        /// Clears the parameters for this stored procedure
        /// </summary>
        public void ResetParameters()
        {
            mSqlCommand.Parameters.Clear();
            mHasDerivedParameters = false;
        }

        /// <summary>
        /// Gets or sets the wait time in seconds before terminating the attempt to execute a command and generating an error.
        /// </summary>
        public int CommandTimeout
        {
            get
            {
                return mSqlCommand.CommandTimeout;
            }
            set
            {
                mSqlCommand.CommandTimeout = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the datatable should be filled with its schema.
        /// <example>
        /// The following example shows how to get the shema for a table and write it to a file.
        /// <code>
        /// <![CDATA[
        /// 
        /// ]]>
        /// </code>
        /// </example>
        /// </summary>
        public bool FillSchema
        {
            get
            {
                return mFillSchema;
            }
            set
            {
                mFillSchema = value;
            }
        }

        

        #region DataTable
        /// <summary>
        /// Executes the stored procedure and returns a <see cref="System.Data.DataTable"/>.
        /// </summary>
        /// <returns><see cref="System.Data.DataTable"/></returns>
        public DataTable ExecuteDataTable()
        {
            Connection.ExecutionType executionType = mFillSchema == true ? 
                Connection.ExecutionType.ExecuteDataTableWithSchema : Connection.ExecutionType.ExecuteDataTable;
            return (DataTable)mConnection.ExecuteCommand(executionType, mSqlCommand);
        }

        /// <summary>
        /// Executes the stored procedure and returns a <see cref="System.Data.DataTable"/>.
        /// </summary>
        /// <example>
        /// The following example shows how to execute a stored procedure and populate a <see cref="System.Data.DataTable"/>.
        /// <code>
        /// <![CDATA[
        /// Connection connection = new Connection();
        /// DataTable customerDataTable = connection.StoredProcedures["GetCustomerList"].ExecuteDataTable();
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="parameters">A <see cref="System.Object"/> array containing zero of more parameters values</param>
        /// <returns><see cref="System.Data.DataTable"/></returns>
        public DataTable ExecuteDataTable(params object[] parameters)
        {
            FillParameters(parameters);
            Connection.ExecutionType executionType = mFillSchema == true ?
                Connection.ExecutionType.ExecuteDataTableWithSchema : Connection.ExecutionType.ExecuteDataTable;
            return (DataTable)mConnection.ExecuteCommand(executionType, mSqlCommand);
        }
                
        #endregion

        #region NonQuery
        /// <summary>
        /// Executes the stored procedure and returns the number of rows affected
        /// </summary>
        /// <returns><see cref="System.Int32"/></returns>
        public int ExecuteNonQuery()
        {
            return (int)mConnection.ExecuteCommand(Connection.ExecutionType.ExecuteNonQuery, mSqlCommand);

        }

        

        /// <summary>
        /// Executes the stored procedure and returns the number of rows affected
        /// </summary>
        /// <param name="parameters">A System.Object array containing zero of more parameters values</param>
        /// <returns><see cref="System.Int32"/></returns>
        public int ExecuteNonQuery(params object[] parameters)
        {
            FillParameters(parameters);
            return (int)mConnection.ExecuteCommand(Connection.ExecutionType.ExecuteNonQuery, mSqlCommand);

        }

        #endregion

        #region Reader
        /// <summary>
        /// Execututes the stored procedure and returns a <see cref="System.Data.SqlClient.SqlDataReader"/>.
        /// </summary>
        /// <returns><see cref="System.Data.SqlClient.SqlDataReader"/></returns>
        public SqlDataReader ExecuteReader()
        {
            throw new Exception("The method or operation is not implemented.");
        }

       
        /// <summary>
        /// Execututes the stored procedure and returns a <see cref="System.Data.SqlClient.SqlDataReader"/>.
        /// </summary>
        /// <param name="parameters">A System.Object array containing zero of more parameters values</param>
        /// <returns><see cref="System.Data.SqlClient.SqlDataReader"/></returns>
        public SqlDataReader ExecuteReader(params object[] parameters)
        {
            FillParameters(parameters);
            return (SqlDataReader)mConnection.ExecuteCommand(Connection.ExecutionType.ExecuteReader, mSqlCommand);
        }

       

        #endregion

        #region Scalar
        /// <summary>
        /// Executes the <see cref="DbDotNet.StoredProcedure"/> and returns the first column on the first row.
        /// Extra columns and rows are ignored.
        /// </summary>
        /// <returns><see cref="System.Object"/></returns>
        public object ExecuteScalar()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// Executes the <see cref="DbDotNet.StoredProcedure"/> and returns the first column on the first row.
        /// Extra columns and rows are ignored.
        /// </summary>
        /// <param name="parameters">A <see cref="System.Object"/> array containing zero of more parameters values</param>
        /// <returns><see cref="System.Object"/></returns>
        public object ExecuteScalar(params object[] parameters)
        {
            FillParameters(parameters);
            return mConnection.ExecuteCommand(Connection.ExecutionType.ExecuteScalar, mSqlCommand);
        }

        #endregion

        #region XmlReader
        /// <summary>
        /// Executes the <see cref="DbDotNet.StoredProcedure"/> and returns a <see cref="System.Xml.XmlReader"/>.
        /// </summary>
        /// <returns><see cref="System.Xml.XmlReader"/></returns>
        public XmlReader ExecuteXmlReader()
        {
            throw new Exception("The method or operation is not implemented.");
            
        }

        /// <summary>
        /// Executes the <see cref="DbDotNet.StoredProcedure"/> and returns a <see cref="System.Xml.XmlReader"/>.
        /// </summary>
        /// <param name="parameters">A <see cref="System.Object"/> array containing zero of more parameters values</param>
        /// <returns><see cref="System.Xml.XmlReader"/></returns>
        public XmlReader ExecuteXmlReader(params object[] parameters)
        {
            FillParameters(parameters);
            return (XmlReader)mConnection.ExecuteCommand(Connection.ExecutionType.ExecuteXmlReader, mSqlCommand);
        }

        #endregion

        #endregion

    }
}
