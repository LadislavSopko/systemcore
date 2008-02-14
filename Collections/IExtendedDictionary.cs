using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace System.Common.Collections
{
    /// <summary>
    /// Represents a generic collection providing support for change notifications and serialization.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
    public interface IExtendedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
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
        ///Determines whether the <see cref="IDictionary{TKey,TValue}"></see> contains a specific value.
        ///The key is defined by the type implementing the <see cref="IKeyProvider{T}"/> interface.
        ///</summary> 
        /// <remarks>
        /// This method requires the <paramref name="value"/> to implement the <see cref="IKeyProvider{T}"/> interface.
        /// </remarks>       
        bool Contains<T>(T value) where T : TValue, IKeyProvider<TKey>;


        /// <summary>
        /// Determines the index of a specific item in the <see cref="IExtendedDictionary{TKey,TValue}"/>
        /// </summary>
        /// <param name="value">The object to locate in the <see cref="IExtendedDictionary{TKey,TValue}"/></param>
        /// <returns>The key of the item if found, otherwise Default(TKey) </returns>
        TKey KeyOf(TValue value);


        /// <summary>
        /// Adds a new item to the dictinary
        /// </summary>
        /// <remarks>
        /// This method requires the <paramref name="value"/> to implement the <see cref="IKeyProvider{T}"/> interface.
        /// </remarks>
        /// <typeparam name="T">The type of object to add</typeparam>
        /// <param name="value">The item to be added to the dictionary.</param>
        void Add<T>(T value) where T : TValue, IKeyProvider<TKey>;


        void Add(TValue value);


        

        TValue this[int index] { get; set; }

        
        

        /// <summary>
        /// Removes an item from the dictionary.
        /// </summary>
        /// <remarks>
        /// This method requires the <paramref name="value"/> to implement the <see cref="IKeyProvider{T}"/> interface.
        /// </remarks>
        /// <typeparam name="T">The type of object to remove</typeparam>
        /// <param name="value">The item to be removed from the dictionary.</param>
        bool Remove<T>(T value) where T : TValue, IKeyProvider<TKey>;


        new IEnumerator<TValue> GetEnumerator();
    }
}
