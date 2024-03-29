namespace System.Common.Collections
{
    /// <summary>
    /// Defines the type of change made to an <see cref="IExtendedDictionary{TKey,TValue}"/>.
    /// </summary>
    public enum DictionaryChangedType
    {
        /// <summary>
        /// An item is added to the dictionary.
        /// </summary>
        ItemAdded,
        
        /// <summary>
        /// An item is removed from the dictionary.
        /// </summary>
        ItemRemoved,
        
        /// <summary>
        /// An item in the dictionary is changed.
        /// </summary>
        ItemChanged,

        /// <summary>
        /// The dictionary is cleared
        /// </summary>
        Cleared
    }

    /// <summary>
    /// Provides data for the <see cref="IExtendedDictionary{TKey,TValue}.DictionaryChanged"/> event.
    /// </summary>
    /// <typeparam name="TKey">The key type</typeparam>
    /// <typeparam name="TValue">The item type</typeparam>
    public class DictionaryChangedEventArgs<TKey, TValue> : EventArgs
    {

        #region Private Member Variables

        /// <summary>
        /// The key of the item causing the change in the dictionary.
        /// </summary>
        private readonly TKey _key;

        /// <summary>
        /// The index of the item causing the change in the dictionary.
        /// </summary>
        private readonly int _index;

        /// <summary>
        /// The item that is causing the change in the dictionary.
        /// </summary>
        private readonly TValue _value;

        /// <summary>
        /// The name of the property causing the item to change. 
        /// </summary>
        private readonly string _propertyName;



        /// <summary>
        /// The type of change made to the dictionary.
        /// </summary>
        private readonly DictionaryChangedType _dictionaryChangedType;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="DictionaryChangedEventArgs{TKey,TValue}"/> class.
        /// </summary>
        /// <param name="key">The key of the item causing the change in the dictionary.</param>
        /// <param name="index">The index of the item causing the change in the dictionary.</param>
        /// <param name="value">The item that is causing the change in the dictionary.</param>
        /// <param name="dictionaryChangedType">A <see cref="DictionaryChangedType"/> value indicating the type of change.</param>
        public DictionaryChangedEventArgs(TKey key, int index, TValue value, DictionaryChangedType dictionaryChangedType)
            : this(key, index, value, null, dictionaryChangedType) { }
        

        /// <summary>
        /// Creates a new instance of the <see cref="DictionaryChangedEventArgs{TKey,TValue}"/> class.
        /// </summary>
        /// <param name="key">The key of the item causing the change in the dictionary.</param>
        /// <param name="index">The index of the item causing the change in the dictionary.</param>
        /// <param name="value">The item that is causing the change in the dictionary.</param>
        /// <param name="propertyName">The name of the property causing the item to change.</param>
        /// <param name="dictionaryChangedType">A <see cref="DictionaryChangedType"/> value indicating the type of change.</param>
        public DictionaryChangedEventArgs(TKey key, int index, TValue value, string propertyName, DictionaryChangedType dictionaryChangedType)
        {
            _key = key;
            _index = index;
            _value = value;
            _propertyName = propertyName;
            _dictionaryChangedType = dictionaryChangedType;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the item that is causing the change in the dictionary.
        /// </summary>    
        public TValue Value
        {
            get { return _value; }
        }

        /// <summary>
        /// Gets the key of the item causing the change in the dictionary.
        /// </summary>
        public TKey Key
        {
            get { return _key; }
        }


        /// <summary>
        /// Gets the index of the item causing the change in the dictionary.
        /// </summary>
        public int Index
        {
            get { return _index; }
        }


        /// <summary>
        /// Gets the name of the property causing the item to change. 
        /// </summary>
        public string PropertyName
        {
            get { return _propertyName; }
        }

        /// <summary>
        /// Gets the <see cref="DictionaryChangedType"/> value indicating the type of change.
        /// </summary>
        public DictionaryChangedType DictionaryChangedType
        {
            get { return _dictionaryChangedType; }
        }

        #endregion

    }
}
