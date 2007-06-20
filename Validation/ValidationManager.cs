using System;
using System.Collections.Generic;
using System.Core.Collections;
using System.Reflection;
using ValidatorCollection = System.Core.Collections.HybridCollection<string, System.Collections.Generic.IList<System.Core.Validation.ValidatorBase>>;

namespace System.Core.Validation
{
    public class ValidationManager
    {
        private object _target;
        
        /// <summary>
        /// Stores the Validators for a particular type.
        /// </summary>
        private static HybridCollection<Type, ValidatorCollection> _typeValidators = new HybridCollection<Type, HybridCollection<string, IList<ValidatorBase>>>();

        /// <summary>
        /// Stores the validation errors for the current target
        /// </summary>
        private HybridCollection<string, string> _validationErrors = new HybridCollection<string, string>();
        

        public ValidationManager(object target)
        {
            _target = target;            
        }


        /// <summary>
        /// Performs validation for the current target
        /// </summary>
        /// <param name="validators">The validators.</param>
        private void RunValidators(IList<ValidatorBase> validators)
        {
            //Check to see if there are any validators 
            if (validators == null)
                return;

            //Check to see if any validators failes to validates
            for (int i = 0; i < validators.Count; i++)
            {
                if (!validators[i].IsValid(_target))
                {                    
                    //Failed to validate    
                    _validationErrors[validators[i].PropertyName] = validators[i].ErrorMessage;
                    break;
                }
            }
        }

        public bool IsPropertyValid(string propertyName)
        {        
            //Remove the validation errors before validation
            _validationErrors.Remove(propertyName);
            ValidatorCollection validators = GetValidators(_target.GetType());


            RunValidators(validators[propertyName]);

            return !_validationErrors.Contains(propertyName);
        }


        private static ValidatorCollection GetValidators(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            if (!_typeValidators.Contains(type))
            {
                //Create a new collection to hold the validators for this type
                ValidatorCollection validators = new ValidatorCollection();

                //Get a list of properties on this type
                PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);

                foreach (PropertyInfo property in properties)
                {
                    //Get a list of attributes deriving from ValidatorAttributeBase
                    ValidatorAttributeBase[] validatorAttributes = (ValidatorAttributeBase[])property.GetCustomAttributes(typeof(ValidatorAttributeBase), true);
                    
                    //Create a new list to hold the attributes for this property
                    List<ValidatorAttributeBase> attributeList = new List<ValidatorAttributeBase>(validatorAttributes);
                    
                    //Sort the attributes according to the sequence property 
                    attributeList.Sort(
                        delegate(ValidatorAttributeBase attribute1, ValidatorAttributeBase attribute2)
                        {
                            return attribute1.Sequence.CompareTo(attribute2.Sequence);
                        });

                    //Create the validators for this property
                    List<ValidatorBase> list = attributeList.ConvertAll<ValidatorBase>(
                        delegate(ValidatorAttributeBase attribute)
                        {
                            return attribute.CreateValidator(property.GetType(), property.Name);
                        });

                    
                    //Add the validators to this property
                    validators.Add(property.Name, list);
                }
                //Store the validators for this type 
                _typeValidators.Add(type, validators);
            }

            //Return the list of validators for the requested type
            return _typeValidators[type];
        }


        /// <summary>
        /// Gets the validation errors.
        /// </summary>
        /// <value>The validation errors.</value>
        public HybridCollection<string, string> ValidationErrors
        {
            get { return _validationErrors; }
        }


        public static void AddValidator(Type type,ValidatorBase validator,string propertyName)
        {
            GetValidators(type)[propertyName].Add(validator);
        }
    }
}
