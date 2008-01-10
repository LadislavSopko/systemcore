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
    public class ExtendedDictionary<TKey, TValue> : IExtendedDictionary<TKey, TValue>, IList<TValue>, IList
    {

        #region Private Member Variables

        /// <summary>
        /// Contains the elements in the form of a <see cref="IDictionary{TKey,TValue}"/>.
        /// </summary>
        private readonly IDictionary<TKey, TValue> _dictionary;

        /// <summary>
        /// Contains the elements in the form of a <see cref="IList{T}"/>.
        /// </summary>
        private readonly IList<TValue> _list;


        private readonly IDictionary<int, TKey> _keys;


        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="ExtendedDictionary{TKey,TValue}"/> class.
        /// </summary>
        public ExtendedDictionary()
            : this(new Dictionary<TKey, TValue>()) { }
        

        /// <summary>
        /// Creates a new instance of the <see cref="ExtendedDictionary{TKey,TValue}"/> class 
        /// that contains elements copied from the specified <see cref="IDictionary{TKey,TValue}"/> and uses the default equality comparer for the key type. 
        /// </summary>
        /// <param name="dictionary">The <see cref="IDictionary{TKey,TValue}"/> whose elements are copied to the new <see cref="ExtendedDictionary{TKey,TValue}"/>. </param>
        public ExtendedDictionary(IDictionary<TKey, TValue> dictionary)
        {
            _dictionary = new Dictionary<TKey, TValue>(dictionary);

                        
            _list = new List<TValue>(dictionary.Values);
            _keys = new Dictionary<int, TKey>();

            //Make sure that tje keys collection is in sync with the base collection
            foreach (KeyValuePair<TKey, TValue> pair in _dictionary)
            {
                _keys.Add(_list.IndexOf(pair.Value), pair.Key);
            }
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
            _keys.Add(_list.IndexOf(value), key);
            WirePropertyChanged(value);
            if (DictionaryChanged != null)
            {
                DictionaryChanged(this, new DictionaryChangedEventArgs<TKey, TValue>(key, _list.IndexOf(value), value, DictionaryChangedType.ItemAdded));
            }
        }

        /// <summary>
        /// Called when an item is removed from the dictionary.
        /// </summary>
        /// <param name="key">The key of the item that has been removed from the dictionary.</param>
        /// <param name="value">The item that has been removed.</param>
        private void OnItemRemoved(TKey key, TValue value)
        {
            _keys.Remove(_list.IndexOf(value));
            UnWirePropertyChanged(value);
            OnDictionaryChanged(new DictionaryChangedEventArgs<TKey, TValue>(key, _list.IndexOf(value), value, DictionaryChangedType.ItemRemoved));
        }

        /// <summary>
        /// Validates the <paramref name="type"/> to see if it is valid according to the generic type parameters,<typeparamref name="TKey"/>  and <typeparamref name="TValue"/>. 
        /// </summary>
        /// <param name="type">The type to validate</param>
        private static void ValidateType(Type type)
        {
            ValidateObjectType(type);
            ValidateKeyProvider(type);
        }

        /// <summary>
        /// Validates the <paramref name="type"/> to see if it is valid according to the generic type parameter <typeparamref name="TValue"/>. 
        /// </summary>
        /// <param name="type">The type to validate</param>
        private static void ValidateObjectType(Type type)
        {
            if (!typeof(TValue).IsAssignableFrom(type))
            {
                throw new ArgumentException(string.Format("The supplied value is invalid. The value has to be {0}", typeof(TValue).Name));
            }
        }

        /// <summary>
        /// Validates the <paramref name="type"/> to see if it implements the <see cref="IKeyProvider{T}"/> interface.
        /// </summary>
        /// <param name="type">The type to validate</param>
        private static void ValidateKeyProvider(Type type)
        {
            if (!typeof(IKeyProvider<TKey>).IsAssignableFrom(type))
            {
                throw new ArgumentException(string.Format("The supplied value is invalid. In order to work with object without supplying the key, the object type has to implement IKeyProvider<{0}>", typeof(TKey).Name));
            }
        }


        /// <summary>
        /// Adds an item to the <see cref="_dictionary"/> and the <see cref="_list"/> collections.
        /// </summary>
        /// <param name="key">The key of the item to add</param>
        /// <param name="value">The item to remove</param>
        private int AddItem(TKey key, TValue value)
        {
            _dictionary.Add(key, value);
            int newIndex = ((IList)_list).Add(value);
            OnItemAdded(key, value);
            return newIndex;
        }


        /// <summary>
        /// Removes an item from the <see cref="_dictionary"/> and the <see cref="_list"/> collections.
        /// </summary>
        /// <param name="key">The key of the value to remove</param>
        /// <param name="value">The value to remove</param>
        /// <returns>true if the element is successfully removed; otherwise, false.  This method also returns false if key was not found in the original <see cref="IDictionary{TKey,TValue}"/></returns>        
        private bool RemoveItem(TKey key, TValue value)
        {
            if (_dictionary.Remove(key))
            {
                _list.Remove(value);
                OnItemRemoved(key, value);
                return true;
            }
            return false;
        }

        private void ClearItems()
        {
            _list.Clear();
            _dictionary.Clear();
            _keys.Clear();
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
        /// Raises the <see cref="DictionaryChanged"/> event.
        /// </summary>
        /// <remarks>
        /// In order for this method to be called, the item type has to implement the <see cref="INotifyPropertyChanged"/> interface.
        /// </remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="PropertyChangedEventArgs"/>PropertyChangedEventArgs that contains the event data.</param>
        private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            TKey key;
            if (!typeof(IKeyProvider<TKey>).IsAssignableFrom(sender.GetType()))
                key = default(TKey);
            else
                key = ((IKeyProvider<TKey>)sender).Key;
            OnDictionaryChanged(new DictionaryChangedEventArgs<TKey, TValue>(key, _list.IndexOf((TValue)sender), (TValue)sender, e.PropertyName, DictionaryChangedType.ItemChanged));
        }

        #endregion

        #region Public Methods

        ///// <summary>
        ///// Adds a new item to the dictionary.
        ///// </summary>
        ///// <remarks>
        ///// This method is required to support serialization and should not be used directly from code.
        ///// </remarks>
        ///// <param name="item">The item to be added to the dictionary.</param>
        //public void Add(object item)
        //{
        //    ValidateType(item.GetType());
        //    AddItem(((IKeyProvider<TKey>)item).Key, (TValue)item);
        //}

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
            AddItem(value.Key, value);
        }


        public void Add(TValue value)
        {
            ValidateKeyProvider(value.GetType());
            AddItem(((IKeyProvider<TKey>)value).Key,value);
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


        /// <summary>
        /// Determines the index of a specific item in the <see cref="IExtendedDictionary{TKey,TValue}"/>
        /// </summary>
        /// <param name="value">The object to locate in the <see cref="IExtendedDictionary{TKey,TValue}"/></param>
        /// <returns>The key of the item if found, otherwise Default(TKey) </returns>
        public TKey KeyOf(TValue value)
        {
            if (_keys.ContainsKey(_list.IndexOf(value)))
                return _keys[_list.IndexOf(value)];
            else
                return default(TKey);
        }

        /// <summary>
        /// Removes an item from the dictionary.
        /// </summary>
        /// <remarks>
        /// This method requires the <paramref name="value"/> to implement the <see cref="IKeyProvider{T}"/> interface.
        /// </remarks>
        /// <typeparam name="T">The type of object to remove</typeparam>
        /// <param name="value">The item to be removed from the dictionary.</param>
        public bool Remove<T>(T value) where T : TValue, IKeyProvider<TKey>
        {
            return RemoveItem(value.Key, value);
        }


        public TValue this[int index]
        {
            get { return ((IList<TValue>)this)[index]; }
            set { ((IList<TValue>)this)[index] = value; }
        }


        IEnumerator<TValue> IExtendedDictionary<TKey, TValue>.GetEnumerator()
        {
            return _dictionary.Values.GetEnumerator();
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
            _list.Add(value);
            OnItemAdded(key, value);
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
            get { return _dictionary[key]; }
            set
            {
                _dictionary[key] = value;
                _list[_list.IndexOf(value)] = value;
            }
        }

        ///<summary>
        ///Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2"></see> contains an element with the specified key.
        ///</summary>        
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
            TValue value = _dictionary[key];
            if (_dictionary.Remove(key))
            {
                _list.Remove(value);
                OnItemRemoved(key, value);
                return true;
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
        ///Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        ///</summary>
        ///
        ///<exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only. </exception>
        void ICollection<TValue>.Clear()
        {            
            ClearItems();
        }

        #endregion

        #region ICollection<KeyValuePair<TKey,TValue>> Members

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            AddItem(item.Key, item.Value);
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
            return RemoveItem(item.Key, item.Value);
        }


        ///<summary>
        ///Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        ///</summary>
        ///
        ///<exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only. </exception>
        public void Clear()
        {
            ClearItems();
        }

        #endregion

        #region IEnumerable<KeyValuePair<TKey,TValue>> Members

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return _dictionary.GetEnumerator();
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

        #region IList<TValue> Members

        ///<summary>
        ///Gets or sets the element at the specified index.
        ///</summary>
        ///
        ///<returns>
        ///The element at the specified index.
        ///</returns>
        ///
        ///<param name="index">The zero-based index of the element to get or set.</param>
        ///<exception cref="T:System.ArgumentOutOfRangeException">index is not a valid index in the <see cref="IList{T}"></see>.</exception>
        ///<exception cref="T:System.NotSupportedException">The property is set and the <see cref="IList{T}"></see> is read-only.</exception>
        TValue IList<TValue>.this[int index]
        {
            get
            {
                return _list[index];
            }
            set
            {
                ValidateKeyProvider(typeof(TValue));
                _list[index] = value;
                _dictionary[((IKeyProvider<TKey>)value).Key] = value;
            }
        }

        ///<summary>
        ///Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1"></see>.
        ///</summary>
        ///
        ///<returns>
        ///The index of item if found in the list; otherwise, -1.
        ///</returns>
        ///
        ///<param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1"></see>.</param>
        int IList<TValue>.IndexOf(TValue item)
        {
            return _list.IndexOf(item);
        }


        ///<summary>
        ///Inserts an item to the <see cref="T:System.Collections.Generic.IList`1"></see> at the specified index.
        ///</summary>
        ///
        ///<param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1"></see>.</param>
        ///<param name="index">The zero-based index at which item should be inserted.</param>
        ///<exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1"></see> is read-only.</exception>
        ///<exception cref="T:System.ArgumentOutOfRangeException">index is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"></see>.</exception>
        void IList<TValue>.Insert(int index, TValue item)
        {
            ValidateKeyProvider(typeof(TValue));
            _dictionary.Add(((IKeyProvider<TKey>)item).Key, item);
            _list.Insert(index, item);
            OnItemAdded(((IKeyProvider<TKey>)item).Key, item);
        }

        ///<summary>
        ///Removes the <see cref="T:System.Collections.Generic.IList`1"></see> item at the specified index.
        ///</summary>
        ///
        ///<param name="index">The zero-based index of the item to remove.</param>
        ///<exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1"></see> is read-only.</exception>
        ///<exception cref="T:System.ArgumentOutOfRangeException">index is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"></see>.</exception>
        void IList<TValue>.RemoveAt(int index)
        {
            RemoveItem(((IKeyProvider<TKey>)_list[index]).Key, _list[index]);
        }

        #endregion

        #region ICollection<TValue> Members



        ///<summary>
        ///Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        ///</summary>
        ///
        ///<param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        ///<exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.</exception>
        void ICollection<TValue>.Add(TValue item)
        {
            ValidateKeyProvider(typeof(TValue));
            AddItem(((IKeyProvider<TKey>)item).Key, item);
        }




        ///<summary>
        ///Removes the specific object from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        ///</summary>
        ///
        ///<returns>
        ///true if item was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false. This method also returns false if item is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        ///</returns>
        ///
        ///<param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        ///<exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.</exception>
        public bool Remove(TValue item)
        {
            ValidateKeyProvider(typeof(TValue));
            return RemoveItem(((IKeyProvider<TKey>)item).Key, item);
        }


        ///<summary>
        ///Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> contains a specific value.
        ///</summary>
        ///
        ///<returns>
        ///true if item is found in the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false.
        ///</returns>
        ///
        ///<param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        public bool Contains(TValue item)
        {
            return _list.Contains(item);
        }


        ///<summary>
        ///Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"></see> to an <see cref="T:System.Array"></see>, starting at a particular <see cref="T:System.Array"></see> index.
        ///</summary>
        ///
        ///<param name="array">The one-dimensional <see cref="T:System.Array"></see> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"></see>. The <see cref="T:System.Array"></see> must have zero-based indexing.</param>
        ///<param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        ///<exception cref="T:System.ArgumentOutOfRangeException">arrayIndex is less than 0.</exception>
        ///<exception cref="T:System.ArgumentNullException">array is null.</exception>
        ///<exception cref="T:System.ArgumentException">array is multidimensional.-or-arrayIndex is equal to or greater than the length of array.-or-The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1"></see> is greater than the available space from arrayIndex to the end of the destination array.-or-Type T cannot be cast automatically to the type of the destination array.</exception>
        public void CopyTo(TValue[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }


        #endregion

        #region IList Members

        int IList.Add(object value)
        {
            ValidateType(value.GetType());
            return AddItem(((IKeyProvider<TKey>)value).Key, (TValue)value);
        }

        void IList.Clear()
        {
            ClearItems();
        }

        ///<summary>
        ///Determines whether the <see cref="T:System.Collections.IList"></see> contains a specific value.
        ///</summary>
        ///
        ///<returns>
        ///true if the <see cref="T:System.Object"></see> is found in the <see cref="T:System.Collections.IList"></see>; otherwise, false.
        ///</returns>
        ///
        ///<param name="value">The <see cref="T:System.Object"></see> to locate in the <see cref="T:System.Collections.IList"></see>. </param><filterpriority>2</filterpriority>
        bool IList.Contains(object value)
        {
            return ((IList)_list).Contains(value);
        }

        int IList.IndexOf(object value)
        {
            return ((IList)_list).IndexOf(value);
        }

        void IList.Insert(int index, object value)
        {
            ValidateType(value.GetType());
            _dictionary.Add(((IKeyProvider<TKey>)value).Key, (TValue)value);
            _list.Insert(index, (TValue)value);
            OnItemAdded(((IKeyProvider<TKey>)value).Key, (TValue)value);
        }

        bool IList.IsFixedSize
        {
            get { return ((IList)_list).IsFixedSize; }
        }

        bool IList.IsReadOnly
        {
            get { return ((IList)_list).IsReadOnly; }
        }

        void IList.Remove(object value)
        {
            ValidateType(value.GetType());
            RemoveItem(((IKeyProvider<TKey>)value).Key, (TValue)value);
        }

        void IList.RemoveAt(int index)
        {
            ((IList<TValue>)this).RemoveAt(index);
        }

        object IList.this[int index]
        {
            get
            {
                return ((IList<TValue>)this)[index];
            }
            set
            {
                ValidateType(value.GetType());
                ((IList<TValue>)this)[index] = (TValue)value;
            }
        }

        #endregion

        #region ICollection Members

        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection)_list).CopyTo(array, index);
        }

        int ICollection.Count
        {
            get { return ((ICollection)_list).Count; }
        }

        bool ICollection.IsSynchronized
        {
            get { return ((ICollection)_list).IsSynchronized; }
        }

        object ICollection.SyncRoot
        {
            get { return ((ICollection)_list).SyncRoot; }
        }

        #endregion
    }
}
