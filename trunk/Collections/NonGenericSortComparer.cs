using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Collections;

namespace System.Common.Collections
{
    public class NonGenericSortComparer : IComparer
    {
        private PropertyDescriptor _propertyDescriptor = null;
        private ListSortDirection _direction;

        public NonGenericSortComparer(PropertyDescriptor propDesc,
           ListSortDirection direction)
        {
            _propertyDescriptor = propDesc;
            _direction = direction;
        }


        #region IComparer Members

        public int Compare(object x, object y)
        {
            int result = 0;

            object xValue = _propertyDescriptor.GetValue(x);
            object yValue = _propertyDescriptor.GetValue(y);    
            
            if (xValue is IComparable)
            {
                result = ((IComparable)xValue).CompareTo(yValue);           
            }
            else if (yValue is IComparable)
            {
                result = ((IComparable)yValue).CompareTo(xValue);           
            }

            if (_direction == ListSortDirection.Ascending)
            {
                return result;
            }
            else
            {
                return result * -1;
            }
        }

        #endregion
    }
}
