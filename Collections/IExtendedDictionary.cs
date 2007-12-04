using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace System.Core.Collections
{
    /// <summary>
    /// Represents a generic collection providing support for change notifications and serialization.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
    public interface IExtendedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IEnumerable<TValue>
    {
        /// <summary>
        /// This event is raised when the dictionary changed.
        /// </summary>        
        event EventHandler<DictionaryChangedEventArgs<TKey, TValue>> DictionaryChanged;

        /// <summary>
        /// This event is raised when an item is changed.
        /// </summary>
        /// <remarks>
        /// In order for this event to be raised, the item type has to implement the <see cref="INotifyPropertyChanged"/> interface.
        /// </remarks>
        event EventHandler<PropertyChangedEventArgs> ItemChanged;

        ///<summary>
        ///Determines whether the <see cref="IDictionary{TKey,TValue}"></see> contains an element with the specified key.
        ///The key is defined by the type implementing the <see cref="IKeyProvider{T}"/> interface.
        ///</summary> 
        /// <remarks>
        /// This method requires the <paramref name="value"/> to implement the <see cref="IKeyProvider{T}"/> interface.
        /// </remarks>       
        bool Contains<T>(T value) where T : TValue, IKeyProvider<TKey>;

        /// <summary>
        /// Adds a new item to the dictinary
        /// </summary>
        /// <remarks>
        /// This method requires the <paramref name="value"/> to implement the <see cref="IKeyProvider{T}"/> interface.
        /// </remarks>
        /// <typeparam name="T">The type of object to add</typeparam>
        /// <param name="value">The item to be added to the dictionary.</param>
        void Add<T>(T value) where T : TValue, IKeyProvider<TKey>;

        /// <summary>
        /// Removes an item from the dictionary.
        /// </summary>
        /// <remarks>
        /// This method requires the <paramref name="value"/> to implement the <see cref="IKeyProvider{T}"/> interface.
        /// </remarks>
        /// <typeparam name="T">The type of object to remove</typeparam>
        /// <param name="value">The item to be removed from the dictionary.</param>
        void Remove<T>(T value) where T : TValue, IKeyProvider<TKey>;

        ///<summary>
        ///Removes the element with the specified key from the <see cref="IDictionary{TKey,TValue}"></see>.
        ///</summary>
        ///<remarks>
        /// The new keyword is needed on this method in order to be called through the <see cref="IExtendedDictionary{TKey,TValue}"/> interface.
        /// </remarks>
        ///
        ///<returns>
        ///true if the element is successfully removed; otherwise, false.  This method also returns false if key was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
        ///</returns>
        ///
        ///<param name="key">The key of the element to remove.</param>
        ///<exception cref="T:System.NotSupportedException">The <see cref="IDictionary{TKey,TValue}"></see> is read-only.</exception>
        ///<exception cref="T:System.ArgumentNullException">key is null.</exception>
        new bool Remove(TKey key);

        /// <summary>
        /// Provides direct access to the generic <see cref="ICollection{T}.GetEnumerator"/> through the <see cref="IExtendedDictionary{TKey,TValue}"/>  interface.
        /// </summary>
        /// <returns></returns>
        new IEnumerator<TValue> GetEnumerator();
    }
}
