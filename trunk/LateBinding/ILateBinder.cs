using System;
namespace System.Core
{
    public interface ILateBinder
    {
        object GetFieldValue(object target, string fieldName);
        object GetPropertyValue(object target, string propertyName);
        void SetFieldValue(object target, string fieldName, object value);
        void SetPropertyValue(object target, string propertyName, object value);
        object this[object target, string propertyName] { get; set; }
    }
}
