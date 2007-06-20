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

        public LengthValidator(PropertyInfo propertyInfo, uint maxLength)
            : this(propertyInfo, 0, maxLength, null)
        {
        }

        public LengthValidator(PropertyInfo propertyInfo,uint maxLength,string errorMessage) 
            : this(propertyInfo,0,maxLength,errorMessage)
        {
        }

        public LengthValidator(PropertyInfo propertyInfo, uint minLength, uint maxLength)
            : this(propertyInfo,minLength,maxLength,null)
        {         
        }

        public LengthValidator(PropertyInfo propertyInfo,uint minLength,uint maxLength,string errorMessage) : base(propertyInfo, errorMessage)
        {
            _minLength = minLength;
            _maxLength = maxLength;
            this.ErrorMessage = "Wrong length";
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
