using System;
using System.Collections.Generic;
using System.Text;

namespace System.Core.Collections
{
    /// <summary>
    /// A type implements this interface to provide a default key in 
    /// implentators of the <see cref="IExtendedDictionary{TKey,TValue}"/> interface.
    /// </summary>
    /// <typeparam name="T">The key type</typeparam>
    public interface IKeyProvider<T>
    {
        /// <summary>
        /// Returns the key.
        /// </summary>
        T Key { get; }
    }
    
}
