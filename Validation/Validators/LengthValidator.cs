using System;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System.Text;


namespace System.Core.Validation
{
    public class LengthValidator : ValidatorBase
    {
        private uint _minLength;
        private uint _maxLength;


        public LengthValidator(Type propertyType, string propertyName, string errorMessage, uint minLength, uint maxLength) : base(propertyType, propertyName, errorMessage)
        {
            _minLength = minLength;
            _maxLength = maxLength;
        }
        
        public LengthValidator(Type propertyType, string propertyName, uint minLength, uint maxLength)
            : base(propertyType, propertyName, null)
        {
            _minLength = minLength;
            _maxLength = maxLength;
        }                

        /// <summary>
        /// Gets the minimum length.
        /// </summary>
        public uint MinLength
        {
            get { return _minLength; }
        }

        /// <summary>
        /// Gets the minimum length
        /// </summary>
        public uint MaxLength
        {
            get { return _maxLength; }
        }

        protected override bool Validate(object value)
        {
            if (value == null)
                return false;

            ICollection col = value as ICollection;
            if (col != null)
                return IsValidLength((uint)col.Count);

            string s = value as string;
            if (s != null)
                return IsValidLength((uint)s.Length);

            return false;
        }

        private bool IsValidLength(uint length)
        {
            return _minLength <= length && length <= _maxLength;
        }
    

        protected override Type[] ValidPropertyTypes
        {
            get { return new Type[] { typeof(ICollection), typeof(string) }; }
        }
    }
}
