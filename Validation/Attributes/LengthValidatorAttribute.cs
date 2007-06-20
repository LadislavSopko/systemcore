using System;
using System.Collections.Generic;
using System.Text;

namespace System.Core.Validation
{
    public class LengthValidatorAttribute : ValidatorAttributeBase
    {
        private uint _minLength;
        private uint _maxLength;
        




        /// <summary>
        /// Initializes a new instance of the <see cref="LengthValidatorAttribute"/> class.
        /// </summary>
        /// <param name="maxLength">The maximum length.</param>
        public LengthValidatorAttribute(uint maxLength)
            : this(uint.MinValue, maxLength)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LengthValidatorAttribute"/> class.
        /// </summary>
        /// <param name="minLength">The minimum length..</param>
        /// <param name="maxLength">The maximum length.</param>
        public LengthValidatorAttribute(uint minLength, uint maxLength)
        {
            _minLength = minLength;
            _maxLength = maxLength;
        }

        public override ValidatorBase CreateValidator(System.Reflection.PropertyInfo propertyInfo)
        {
            return new LengthValidator(propertyInfo, _minLength, _maxLength, ErrorMessage);
        }
    }
}
