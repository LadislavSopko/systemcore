using System;
using System.Collections.Generic;
using System.Reflection;



namespace System.Core.Validation
{
    /// <summary>
    /// Base class for implementing validation attributes
    /// </summary>
    public abstract class ValidatorAttributeBase : Attribute
    {

        #region Private Fields

        private string _errorMessage;
        private uint _sequenceNr;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        /// <value>The error message.</value>
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; }
        }

        /// <summary>
        /// Gets or sets the sequence of validation for this property
        /// </summary>
        /// <value>The order.</value>
        public uint Sequence
        {
            get { return _sequenceNr; }
            set { _sequenceNr = value; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates a validator corresponding to the the type of validator attribute
        /// </summary>
        /// <param name="propertyInfo">The property info.</param>
        /// <returns></returns>
        public abstract ValidatorBase CreateValidator(PropertyInfo propertyInfo);

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidatorAttributeBase"/> class.
        /// </summary>
        public ValidatorAttributeBase()
        {
            this._errorMessage = null;
            this._sequenceNr = 0;
        }

        #endregion

    }
}
