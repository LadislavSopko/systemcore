using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Text;

namespace System.Core.Design
{
    public class LookupDropDownDataMemberConverter : StringConverter
    {
        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            ISelectionService selectionService = (ISelectionService)context.GetService(typeof(ISelectionService));
            object dataSource = TypeDescriptor.GetProperties(selectionService.PrimarySelection)["DataSource"].GetValue(selectionService.PrimarySelection);
            ITypedList listSource = (dataSource as ITypedList);
            if (listSource == null)
            {
                return new StandardValuesCollection(null);
            }
            PropertyDescriptorCollection properties = listSource.GetItemProperties(null);

            string[] members = new string[properties.Count];
            for (int i = 0; i < properties.Count; i++)
            {
                
                members[i] = properties[i].Name;
            }
            return new StandardValuesCollection(members);
        }


       

        

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;
            return base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
                return true;
            return base.CanConvertTo(context, destinationType);
        }

        public override bool IsValid(ITypeDescriptorContext context, object value)
        {
            return true;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
    }
}
