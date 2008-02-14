using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace System.Common.ComponentModel
{
    public class CustomPropertyAttribute : Attribute
    {
        private PropertyDescriptor _propertyDescriptior;
        private string _propertyPath;
        private Type _rootType;


        public CustomPropertyAttribute(Type rootType,string propertyPath, PropertyDescriptor propertyDescriptor)
        {
            _rootType = rootType;
            _propertyPath = propertyPath;
            _propertyDescriptior = propertyDescriptor;                    
        }


        public PropertyDescriptor PropertyInfo
        {
            get { return _propertyDescriptior; }
        }

        public string PropertyPath
        {
            get { return _propertyPath; }
        }

        public Type RootType
        {
            get { return _rootType; }
        }
    }
}
