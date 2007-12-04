using System.ComponentModel;
using System.Core.Emit;
using System.Core.Helpers;

namespace System.Core.ComponentModel
{
    
    /// <summary>
    /// Implements a <see cref="TypeDescriptor"/> describing a nested/custom property.
    /// </summary>
    public class CustomPropertyDescriptor : PropertyDescriptor
    {
        
        #region Private Member Variables

        private readonly PropertyDescriptor  _originalPropertyDescriptor;
        private readonly string _propertyPath;
        private bool _autoCreateObjects;


        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the CustomPropertyDescriptor class.
        /// </summary>
        /// <param name="propertyPath"></param>
        /// <param name="originalPropertyDescriptor"></param>
        /// <param name="attrs"></param>
        public CustomPropertyDescriptor(string propertyPath, PropertyDescriptor originalPropertyDescriptor, Attribute[] attrs,bool autoCreateObjects)
            : base(propertyPath.Replace(".", "_"),attrs)
        {
       
            _propertyPath = propertyPath;
            
            _originalPropertyDescriptor = originalPropertyDescriptor;

            _autoCreateObjects = autoCreateObjects;
        }

        #endregion



        public override bool CanResetValue(object component)
        {
            object instance = GetNestedObjectInstance(component,_propertyPath,false);
            if (instance != null)
                return _originalPropertyDescriptor.CanResetValue(instance);
            else
                return false;
        }


        /// <summary>
        /// Gets the type of the component this property is bound to.        
        /// </summary>        
        public override Type ComponentType
        {
            get { return _originalPropertyDescriptor.ComponentType; }
        }


        /// <summary>
        /// Gets the current value of the property on a component
        /// </summary>
        /// <param name="component">The component with the property for which to retrieve the value.</param>
        /// <returns>The value of a property for a given component.</returns>
        public override object GetValue(object component)
        {
            object instance = GetNestedObjectInstance(component,_propertyPath,false);
            if (instance != null)           
                return DynamicAccessorFactory.GetDynamicAccessor(instance.GetType()).GetPropertyValue(instance, _originalPropertyDescriptor.Name);            
            else
                return null;
        }

        /// <summary>
        /// Sets the value of the component to a different value.
        /// </summary>
        /// <param name="component">The component with the property value that is to be set.</param>
        /// <param name="value">The new value.</param>
        public override void SetValue(object component, object value)
        {
            object instance = GetNestedObjectInstance(component,_propertyPath,_autoCreateObjects);
            if (instance != null)
            {
                DynamicAccessorFactory.GetDynamicAccessor(instance.GetType()).SetPropertyValue(instance, _originalPropertyDescriptor.Name, value);
            }
        }
        

        

        private static object GetNestedObjectInstance(object component,string propertyPath, bool autoCreate)
        {
            string propertyName;
            
            if (propertyPath.Contains("."))
                propertyName = propertyPath.Substring(0, propertyPath.IndexOf("."));
            else
            {
                return component;
            }
            
            IDynamicAccessor dynamicAccessor = DynamicAccessorFactory.GetDynamicAccessor(component.GetType());
            object value = dynamicAccessor.GetPropertyValue(component,propertyName);
            if (value == null)
            {
                if (autoCreate)
                {
                    PropertyDescriptor descriptor = ReflectionHelper.GetPropertyDescriptorFromPath(component.GetType(), propertyName);
                    value = Activator.CreateInstance(descriptor.PropertyType);            
                    dynamicAccessor.SetPropertyValue(component, propertyName, value);
                }
                else
                    return value;
            }
            return GetNestedObjectInstance(value, propertyPath.Substring(propertyPath.IndexOf(".") + 1),autoCreate);
        }



        /// <summary>
        /// Gets a value indicating whether this property is read-only.
        /// </summary>
        public override bool IsReadOnly
        {
            get
            {
                return _originalPropertyDescriptor.IsReadOnly;    
            }
        }

        /// <summary>
        /// Gets the type of the property.
        /// </summary>
        public override Type PropertyType
        {
            get { return _originalPropertyDescriptor.PropertyType; }
        }


        public override void ResetValue(object component)
        {
            object instance = GetNestedObjectInstance(component,_propertyPath,false);
            if (instance != null)
                _originalPropertyDescriptor.ResetValue(instance);
            
        }
       
        /// <summary>
        /// Determines a value indicating whether the value of this property needs to be persisted. 
        /// </summary>
        /// <param name="component">The component with the property to be examined for persistence</param>
        /// <returns>false</returns>
        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }

        public override string ToString()
        {
            return string.Format("Name: {0} , PropertyPath: {1}", Name, _propertyPath);
        }
    }
}
