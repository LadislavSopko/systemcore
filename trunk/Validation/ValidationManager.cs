using System;
using System.Collections.Generic;
using System.Common.Collections;
using System.Reflection;
using PropertyValidators = System.Common.Collections.HybridCollection<string, System.Collections.Generic.IList<System.Common.Validation.ValidatorBase>>;

namespace System.Common.Validation
{
    public class ValidationManager
    {
        private object _target;
        
        /// <summary>
        /// Stores the Validators for a particular type.
        /// </summary>

        private static HybridCollection<Type, PropertyValidators> _typeValidators = new HybridCollection<Type, PropertyValidators>();

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

        public bool IsValid()
        {
            this.ValidationErrors.Clear();
            foreach(IList<ValidatorBase> validators in _typeValidators[_target.GetType()])
            {
                RunValidators(validators);
            }
            return _validationErrors.Count == 0;
        }



        public bool IsPropertyValid(string propertyName)
        {        
            //Remove the validation errors before validation
            _validationErrors.Remove(propertyName);
            PropertyValidators validators = GetValidators(_target.GetType());


            RunValidators(validators[propertyName]);

            return !_validationErrors.Contains(propertyName);
        }





        private static PropertyValidators GetValidators(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            if (!_typeValidators.Contains(type))
            {
                //Create a new collection to hold the validators for this type
                PropertyValidators validators = new PropertyValidators();

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

                    if (list.Count > 0)
                    {
                        //Add the validators to this property
                        validators.Add(property.Name, list);
                    }

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

        public string ValidationError
        {
            get
            {
                List<string> errorList = new List<string>(_validationErrors);
                return string.Join(Environment.NewLine, errorList.ToArray()); 
            }
        }


        public static void AddValidator(Type type,ValidatorBase validator,string propertyName)
        {
            PropertyValidators list = GetValidators(type);
            if (!list.Contains(validator.PropertyName))
            {
                list.Add(propertyName, new List<ValidatorBase>());
            }

            GetValidators(type)[propertyName].Add(validator);
        }
    }
}
