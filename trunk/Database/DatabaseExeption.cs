using System;
using System.Data.SqlClient;
using System.Runtime.Serialization;
using System.Security.Permissions;
namespace System.Core.Database
{
	/// <summary>
	/// This exception is thrown DbDotNet encounters an error.
	/// If the exception originates from SQL Server or other sources,
	/// the original exception is available through inner exeception
	/// </summary>
	/// <remarks>
	/// This means that this is the only exception you need to catch from 
	/// the calling method.
	/// </remarks>
	/// <example>
	/// <code>
	/// Connection conn = new Connection();
	/// try
	/// {
	///		conn.ExecuteNonQuery();
	///	}
	///	catch (DatabaseException ex)
	///	{
	///		Console.WriteLine(ex.ToString);	
	///	}	
	/// </code>
	/// </example>
	[Serializable]
	public sealed class DatabaseException : Exception
	{
		/// <summary>
		/// NETBios name of this local computer 
		/// </summary>
		private string mMachineName = Environment.MachineName;

		/// <summary>
		/// Creates a new instance of the DatabaseException class
		/// </summary>
		public DatabaseException(){}
		
		/// <summary>
		/// Creates a new instance of the DatabaseException class
		/// </summary>
		public DatabaseException(string message) :base(message){}
		
		/// <summary>
		/// Creates a new instance of the DatabaseException class
		/// </summary>
		public DatabaseException(string message, Exception inner) : base(message,inner){}

		
		private DatabaseException(SerializationInfo info, StreamingContext context) : base(info,context)
		{
			if (info !=null) 
			{
				mMachineName = info.GetString("mMachineName");
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="info"></param>
		/// <param name="context"></param>
		[SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter=true)]
		public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			if (info != null)
			{
				info.AddValue("MachineName",mMachineName);
			
			}
			base.GetObjectData (info, context);
		}


		/// <summary>
		/// Gets the machine name of witch this exception occurred
		/// </summary>
		public string MachineName
		{
			get
			{
				return mMachineName;
			}
		}
		
	}
}
