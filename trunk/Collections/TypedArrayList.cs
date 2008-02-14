using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace System.Common.Collections
{
    public class TypedArrayList : ArrayList, ITypedList
    {
        PropertyDescriptorCollection _propertyDescriptors;

        public TypedArrayList(ICollection collection,PropertyDescriptorCollection propertyDescriptors) : base(collection)
        {
            _propertyDescriptors = propertyDescriptors;
        }

        #region ITypedList Members

        public PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
        {
            return _propertyDescriptors;
        }

        public string GetListName(PropertyDescriptor[] listAccessors)
        {
            return null;
        }

        #endregion
    }
}
