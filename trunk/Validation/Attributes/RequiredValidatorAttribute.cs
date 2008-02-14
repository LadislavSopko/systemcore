using System;
using System.Collections.Generic;
using System.Text;

namespace System.Common.Validation.Attributes
{
    public class RequiredValidatorAttribute : ValidatorAttributeBase
    {
        public override ValidatorBase CreateValidator(Type propertyType, string propertyName)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
