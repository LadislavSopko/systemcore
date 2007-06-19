using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Threading;


namespace System.Core.Database
{
	/// <summary>
	/// This class is used to cache parameters based 
	/// on the connectionstring and procedurename
	/// </summary>
	internal  class ParameterCache
	{
		/// <summary>
		/// A synchronized hashtable used to cache the parameters.
		/// </summary>
		private static readonly Hashtable mHashTable = Hashtable.Synchronized(new Hashtable());
		
		/// <summary>
		/// Default constructor
		/// </summary>
		private ParameterCache(){}

		/// <summary>
		/// Gets whether the given command has a cached parameter set 
		/// </summary>
		/// <param name="command">The command to check for cached parameters</param>
		/// <returns>True if the command exists in the cache, otherwise it return false</returns>
		internal static bool IsParametersCached(SqlCommand command)
		{
				string key = command.Connection.ConnectionString + ":" + command.CommandText;	
				return mHashTable.Contains(key);		
		}

		
		/// <summary>
		/// Adds the given command's parameters to the parameter cache 
		/// </summary>
		/// <param name="command">The command that holds the parameters to be cached</param>
		internal static void CacheParameters(SqlCommand command)
		{	
			IDataParameter[] originalParameters = new IDataParameter[command.Parameters.Count];
			command.Parameters.CopyTo(originalParameters,0);
			IDataParameter[] parameters = CloneParameters(originalParameters);
			mHashTable[command.Connection.ConnectionString + ":" + command.CommandText] = parameters;
		}

		/// <summary>
		/// Gets a array of IDataParameter for the given command
		/// </summary>
		/// <param name="command">The command to get cached parameters</param>
		/// <returns>An array of IDataParameter</returns>
		internal static IDataParameter[] GetCachedParameters(SqlCommand command)
		{	
			IDataParameter[] originalParameters = (IDataParameter[])mHashTable[command.Connection.ConnectionString + ":" + command.CommandText];
			return CloneParameters(originalParameters);				
		}


		/// <summary>
		/// Used to create a copy of an array of IDataParameter
		/// </summary>
		/// <param name="originalParameters">The array of IDataParameter we want to copy</param>
		/// <returns>An array of IDataParameter</returns>
		private static IDataParameter[] CloneParameters(IDataParameter[] originalParameters)
		{
			IDataParameter[] clonedParameters = new IDataParameter[originalParameters.Length];

			for (int i = 0, j = originalParameters.Length; i < j; i++)
			{
				clonedParameters[i] = (IDataParameter)((ICloneable)originalParameters[i]).Clone();
			}

			return clonedParameters;
		}
	}
}
