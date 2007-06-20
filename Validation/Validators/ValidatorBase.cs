using System;
using System.Collections.Generic;
using System.Core;
using System.Core.LateBinding;
using System.Reflection;
using System.Text;

namespace System.Core.Validation
{
    /// <summary>
    /// Base class for implementing validators
    /// </summary>
    public abstract class ValidatorBase
    {
        private string _errorMessage;
        private PropertyInfo _propertyInfo;
        private ILateBinder _latebinder;
        
        protected ValidatorBase(PropertyInfo propertyInfo,string errorMessage)
        {
            if (propertyInfo == null)
                throw new ArgumentNullException("propertyInfo");
            
            _propertyInfo = propertyInfo;
            _errorMessage = errorMessage;
            _latebinder = LateBinderFactory.GetLateBinder(propertyInfo.DeclaringType);          
        }


        internal PropertyInfo PropertyInfo
        {
            get { return _propertyInfo; }
        }

        protected ILateBinder Latebinder
        {
            get { return _latebinder; }
        }


        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; }
        }

        /// <summary>
        /// Performs the validation
        /// </summary>
        /// <param name="value">The value to be validated</param>
        /// <returns></returns>
        protected abstract bool Validate(object value);


        /// <summary>
        /// Gets the valid property types.
        /// </summary>
        /// <value>The valid property types.</value>    
        protected abstract Type[] ValidPropertyTypes { get; }


        /// <summary>
        /// Determines whether the specified instance is valid.
        /// </summary>
        /// <param name="instance">The instance declaring the property to be validated</param>
        /// <returns>
        /// 	<c>true</c> if the specified instance is valid; otherwise, <c>false</c>.
        /// </returns>
        public bool IsValid(object instance)
        {
            if (instance == null)
                throw new ArgumentNullException("instance");

            object value = _latebinder.GetPropertyValue(instance, _propertyInfo.Name);
            return Validate(value);
        }

    }
}
