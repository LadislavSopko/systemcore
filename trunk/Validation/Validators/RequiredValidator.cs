using System;
using System.Collections.Generic;
using System.Text;

namespace System.Core.Validation
{
    public class RequiredValidator : ValidatorBase
    {
        public RequiredValidator(Type propertyType, string propertyName, string errorMessage) : base(propertyType, propertyName, errorMessage)
        {
        }

        public RequiredValidator(Type propertyType, string propertyName)
            : base(propertyType, propertyName, null)
        {
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
