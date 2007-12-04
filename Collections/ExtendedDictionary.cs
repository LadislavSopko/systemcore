using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace System.Core.Collections
{
    /// <summary>
    /// Implements the <see cref="IExtendedDictionary{TKey,TValue}"/> interface providing support for change notifications and serialization.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
    [Serializable]
    public class ExtendedDictionary<TKey, TValue> : IExtendedDictionary<TKey, TValue>
    {

        #region Private Member Variables

        private readonly IDictionary<TKey, TValue> _dictionary;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="ExtendedDictionary{TKey,TValue}"/> class.
        /// </summary>
        public ExtendedDictionary()
        {
            _dictionary = new Dictionary<TKey, TValue>();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ExtendedDictionary{TKey,TValue}"/> class 
        /// that contains elements copied from the specified <see cref="IDictionary{TKey,TValue}"/> and uses the default equality comparer for the key type. 
        /// </summary>
        /// <param name="dictionary">The <see cref="IDictionary{TKey,TValue}"/> whose elements are copied to the new <see cref="ExtendedDictionary{TKey,TValue}"/>. </param>
        public ExtendedDictionary(IDictionary<TKey, TValue> dictionary)
        {
            _dictionary = new Dictionary<TKey, TValue>(dictionary);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Assigns the <see cref="INotifyPropertyChanged.PropertyChanged"/> event to the <see cref="OnItemPropertyChanged"/> event handler.
        /// </summary>
        /// <param name="value">The item implementing the<see cref="INotifyPropertyChanged"/> interface.</param>
        private void WirePropertyChanged(TValue value)
        {
            INotifyPropertyChanged notifyPropertyChanged = (value as INotifyPropertyChanged);
            if (notifyPropertyChanged != null)
            {
                notifyPropertyChanged.PropertyChanged += (OnItemPropertyChanged);
            }
        }

        /// <summary>
        /// Removes the <see cref="INotifyPropertyChanged.PropertyChanged"/> event from the <see cref="OnItemPropertyChanged"/> event handler.
        /// </summary>
        /// <param name="value">The item implementing the<see cref="INotifyPropertyChanged"/> interface.</param>
        private void UnWirePropertyChanged(TValue value)
        {
            INotifyPropertyChanged notifyPropertyChanged = (value as INotifyPropertyChanged);
            if (notifyPropertyChanged != null)
            {
                notifyPropertyChanged.PropertyChanged -= (OnItemPropertyChanged);
            }
        }

        /// <summary>
        /// Called when an item is added to the dictionary.
        /// </summary>
        /// <param name="key">The key of the item that has been added to the dictionary.</param>
        /// <param name="value">The item that has been added to the dictionary.</param>
        private void OnItemAdded(TKey key, TValue value)
        {
            WirePropertyChanged(value);
            if (DictionaryChanged != null)
            {
                DictionaryChanged(this, new DictionaryChangedEventArgs<TKey, TValue>(key, value, DictionaryChangedType.ItemAdded));
            }
        }

        /// <summary>
        /// Called when an item is removed from the dictionary.
        /// </summary>
        /// <param name="key">The key of the item that has been removed from the dictionary.</param>
        /// <param name="value">The item that has been removed.</param>
        private void OnItemRemoved(TKey key, TValue value)
        {
            UnWirePropertyChanged(value);
            OnDictionaryChanged(new DictionaryChangedEventArgs<TKey, TValue>(key, value, DictionaryChangedType.ItemRemoved));
        }

        #endregion

        #region Protected Methods



        /// <summary>
        /// Raises the <see cref="DictionaryChanged"/> event.
        /// </summary>
        /// <param name="e">A <see cref="DictionaryChangedEventArgs{Tkey,TValue}"/> that contains the event data.</param>
        protected virtual void OnDictionaryChanged(DictionaryChangedEventArgs<TKey, TValue> e)
        {
            if (DictionaryChanged != null)
            {
                DictionaryChanged(this, e);
            }
        }


        /// <summary>
        /// Raises the <see cref="ItemChanged"/> event.
        /// </summary>
        /// <remarks>
        /// In order for this method to be called, the item type has to implement the <see cref="INotifyPropertyChanged"/> interface.
        /// </remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="PropertyChangedEventArgs"/>PropertyChangedEventArgs that contains the event data.</param>
        protected virtual void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (ItemChanged != null)
            {
                ItemChanged(sender, e);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a new item to the dictionary.
        /// </summary>
        /// <remarks>
        /// This method is required to support serialization and should not be used directly from code.
        /// </remarks>
        /// <param name="item">The item to be added to the dictionary.</param>
        public void Add(object item)
        {
            if (!typeof(IKeyProvider<TKey>).IsAssignableFrom(item.GetType()))
            {
                throw new ArgumentException("Object has to implement IKeyProvider<TKey>", "item");
            }
            else                              
                _dictionary.Add(((IKeyProvider<TKey>)item).Key, (TValue)item);
                
        }

        #endregion

        #region IExtendedDictionary<TKey,TValue> Members

        /// <summary>
        /// This event is raised when the dictionary changed.
        /// </summary>        
        public event EventHandler<DictionaryChangedEventArgs<TKey, TValue>> DictionaryChanged;

        /// <summary>
        /// This event is raised when an item is changed.
        /// </summary>
        /// <remarks>
        /// In order for this event to be raised, the item type has to implement the <see cref="INotifyPropertyChanged"/> interface.
        /// </remarks>
        public event EventHandler<PropertyChangedEventArgs> ItemChanged;

        /// <summary>
        /// Adds a new item to the dictinary
        /// </summary>
        /// <remarks>
        /// This method requires the <paramref name="value"/> to implement the <see cref="IKeyProvider{T}"/> interface.
        /// </remarks>
        /// <typeparam name="T">The type of object to add</typeparam>
        /// <param name="value">The item to be added to the dictionary.</param>
        public void Add<T>(T value) where T : TValue, IKeyProvider<TKey>
        {
            Add(value.Key, value);
        }

        /// <summary>
        /// Removes an item from the dictionary.
        /// </summary>
        /// <remarks>
        /// This method requires the <paramref name="value"/> to implement the <see cref="IKeyProvider{T}"/> interface.
        /// </remarks>
        /// <typeparam name="T">The type of object to remove</typeparam>
        /// <param name="value">The item to be removed from the dictionary.</param>
        public void Remove<T>(T value) where T : TValue, IKeyProvider<TKey>
        {
            Remove(value.Key);
        }

        ///<summary>
        ///Determines whether the <see cref="IDictionary{TKey,TValue}"></see> contains an element with the specified key.
        ///The key is defined by the type implementing the <see cref="IKeyProvider{T}"/> interface.
        ///</summary> 
        /// <remarks>
        /// This method requires the <paramref name="value"/> to implement the <see cref="IKeyProvider{T}"/> interface.
        /// </remarks>       
        public bool Contains<T>(T value) where T : TValue, IKeyProvider<TKey>
        {
            return ContainsKey(value.Key);
        }


        #endregion

        #region IEnumerable Members

        ///<summary>
        ///Returns an enumerator that iterates through a collection.
        ///</summary>
        ///
        ///<returns>
        ///An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.
        ///</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((ICollection)_dictionary).GetEnumerator();
        }

        #endregion

        #region IDictionary<TKey,TValue> Members

        ///<summary>
        ///Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
        ///</summary>
        ///
        ///<param name="value">The object to use as the value of the element to add.</param>
        ///<param name="key">The object to use as the key of the element to add.</param>
        ///<exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2"></see> is read-only.</exception>
        ///<exception cref="T:System.ArgumentException">An element with the same key already exists in the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.</exception>
        ///<exception cref="T:System.ArgumentNullException">key is null.</exception>
        public void Add(TKey key, TValue value)
        {          
            _dictionary.Add(key, value);
            OnItemAdded(key, value);
        }

        ///<summary>
        ///Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        ///</summary>
        ///
        ///<exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only. </exception>
        public void Clear()
        {
            _dictionary.Clear();
        }

        ///<summary>
        ///Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        ///</summary>
        ///
        ///<returns>
        ///The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        ///</returns>
        ///
        public int Count
        {
            get { return _dictionary.Count; }
        }

        ///<summary>
        ///Gets or sets the element with the specified key.
        ///</summary>
        ///
        ///<returns>
        ///The element with the specified key.
        ///</returns>
        ///
        ///<param name="key">The key of the element to get or set.</param>
        ///<exception cref="T:System.NotSupportedException">The property is set and the <see cref="T:System.Collections.Generic.IDictionary`2"></see> is read-only.</exception>
        ///<exception cref="T:System.ArgumentNullException">key is null.</exception>
        ///<exception cref="T:System.Collections.Generic.KeyNotFoundException">The property is retrieved and key is not found.</exception>
        public TValue this[TKey key]
        {
            get { return _dictionary[key];}
            set { _dictionary[key] = value; }
        }

        ///<summary>
        ///Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2"></see> contains an element with the specified key.
        ///</summary>
        ///
        ///<returns>
        ///true if the <see cref="T:System.Collections.Generic.IDictionary`2"></see> contains an element with the key; otherwise, false.
        ///</returns>
        ///
        ///<param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.</param>
        ///<exception cref="T:System.ArgumentNullException">key is null.</exception>
        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        ///<summary>
        ///Gets an <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
        ///</summary>
        ///
        ///<returns>
        ///An <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the keys of the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
        ///</returns>
        ///
        public ICollection<TKey> Keys
        {
            get { return _dictionary.Keys; }
        }

        ///<summary>
        ///Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
        ///</summary>
        ///
        ///<returns>
        ///true if the element is successfully removed; otherwise, false.  This method also returns false if key was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
        ///</returns>
        ///
        ///<param name="key">The key of the element to remove.</param>
        ///<exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IDictionary`2"></see> is read-only.</exception>
        ///<exception cref="T:System.ArgumentNullException">key is null.</exception>
        public bool Remove(TKey key)
        {
            if (_dictionary.ContainsKey(key))
            {
                TValue value = _dictionary[key];
                if (_dictionary.Remove(key))
                {
                    OnItemRemoved(key, value);
                    return true;
                }
                else
                    return false;
            }
            else
                return false;
        }

        /// <summary>
        /// Gets the value associated with the specified key. 
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
        /// <returns>true if the <see cref="IExtendedDictionary{TKey,TValue}"/> contains an element with the specified key; otherwise, false.</returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        ///<summary>
        ///Gets an <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
        ///</summary>
        ///
        ///<returns>
        ///An <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the values in the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
        ///</returns>
        ///
        public ICollection<TValue> Values
        {
            get { return _dictionary.Values; }
        }

        #endregion

        #region ICollection<KeyValuePair<TKey,TValue>> Members

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            _dictionary.Add(item);
            OnItemAdded(item.Key, item.Value);
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _dictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            _dictionary.CopyTo(array, arrayIndex);
        }

        public bool IsReadOnly
        {
            get { return _dictionary.IsReadOnly; }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (_dictionary.Remove(item))
            {
                OnItemRemoved(item.Key, item.Value);
                return true;
            }
            else
                return false;

        }

        #endregion

        #region IEnumerable<KeyValuePair<TKey,TValue>> Members

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return ((IEnumerator<KeyValuePair<TKey, TValue>>)_dictionary);
        }

        #endregion

        #region IEnumerable<TValue> Members

        ///<summary>
        ///Returns an enumerator that iterates through the collection.
        ///</summary>
        ///
        ///<returns>
        ///A <see cref="IEnumerator{T}"></see> that can be used to iterate through the collection.
        ///</returns>
        ///<filterpriority>1</filterpriority>
        public IEnumerator<TValue> GetEnumerator()
        {
            return _dictionary.Values.GetEnumerator();
        }

        #endregion

    }
}
