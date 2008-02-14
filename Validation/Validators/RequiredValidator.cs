using System;
using System.Collections.Generic;
using System.Text;

namespace System.Common.Validation
{
    public class RequiredValidator : ValidatorBase
    {

        public RequiredValidator(Type propertyType, string propertyName)
            : this(propertyType, propertyName, null)
        {
        }
        
        public RequiredValidator(Type propertyType, string propertyName, string errorMessage) : base(propertyType, propertyName, errorMessage)
        {
            if (string.IsNullOrEmpty(errorMessage))
                this.ErrorMessage = string.Format(DefaultErrorMessages.Required, propertyName);
        }

        

        protected override bool Validate(object value)
        {
            return value != null;
        }

        protected override Type[] ValidPropertyTypes
        {
            get { return null; }
        }
    }
}
