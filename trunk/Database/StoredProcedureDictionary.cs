using System;
using System.Collections;

namespace System.Common.Database
{
	/// <summary>
	/// This class is used only by DbDotNet and should not be used directly from code
	/// </summary>
	public class StoredProcedureDictionary : DictionaryBase
	{
		private Connection mConnection;

        /// <summary>
        /// Creates a new instance of the <see cref="DbDotNet.StoredProcedureDictionary"/> class.
        /// </summary>
        /// <param name="connection">The <see cref="DbDotNet.Connection"/> that this <see cref="DbDotNet.StoredProcedureDictionary"/> belongs to.</param>
		internal StoredProcedureDictionary(Connection connection)
		{
			mConnection = connection;
		}
		
		/// <summary>
        /// Returns a <see cref="DbDotNet.StoredProcedure"/> based on the given procedure name.
		/// </summary>
		public StoredProcedure this[string procedureName]
		{
			get
			{
				if (!this.Dictionary.Contains(procedureName))
				{
					this.Dictionary.Add(procedureName, new StoredProcedure(mConnection,procedureName));
				}
				return (StoredProcedure)this.Dictionary[procedureName];
			}
		}
		
		/// <summary>
		/// Copies the elements to an array of type <see cref="DbDotNet.StoredProcedure"/>, staring at the spesified index
		/// </summary>
		/// <param name="array">The destination array to copy to</param>
		/// <param name="index">The zero-based index in array at witch copying begins</param>
		public void CopyTo(StoredProcedure[] array, int index)
		{
			((IDictionary)this).CopyTo(array,index);
		}

	}
}
