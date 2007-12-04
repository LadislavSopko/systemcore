using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;

namespace System.Core.Forms
{
    public class LookupDropDownCell : DataGridViewTextBoxCell
    {
        private object _dataSource;
        

        public object DataSource
        {
            get { return _dataSource; }
            set { _dataSource = value; }
        }


        


        public override object Clone()
        {
            LookupDropDownCell col = base.Clone() as LookupDropDownCell;            
            col.DataSource = _dataSource; 
        
            return col;

        }


        public override Type ValueType
        {
            get
            {
                return typeof(object);
            }
        }


        public override void InitializeEditingControl(int rowIndex, object
        initialFormattedValue, DataGridViewCellStyle dataGridViewCellStyle)
        {
            // Set the value of the editing control to the current cell value.
            base.InitializeEditingControl(rowIndex, initialFormattedValue,
                dataGridViewCellStyle);

            

            LookupDropDownEditingControl ctl =
                DataGridView.EditingControl as LookupDropDownEditingControl;
            ctl.DataSource = ((LookupDropDownColumn)this.OwningColumn).LookupControl.DataSource;
            ctl.DisplayMember = ((LookupDropDownColumn)this.OwningColumn).LookupControl.DisplayMember;
            ctl.DropDownWidth = ((LookupDropDownColumn)this.OwningColumn).LookupControl.DropDownWidth;            
            ctl.EditValue = this.Value;
           
        }


        public override Type EditType
        {
            get
            {
                return typeof(LookupDropDownEditingControl);
            }
        }

        public override object DefaultNewRowValue
        {
            get
            {
                return null;
            }
        }

        protected override void OnClick(DataGridViewCellEventArgs e)
        {
            LookupDropDownEditingControl dropdown = (DataGridView.EditingControl as LookupDropDownEditingControl);
            if (dropdown != null)            
                dropdown.ShowDropDown();
            base.OnClick(e);
        }

        protected override object GetFormattedValue(object value, int rowIndex, ref DataGridViewCellStyle cellStyle, TypeConverter valueTypeConverter, TypeConverter formattedValueTypeConverter, DataGridViewDataErrorContexts context)
        {
            if (value != null)
                return ((LookupDropDownColumn)this.OwningColumn).LookupControl.GetDisplayValue(value);
            else
            {
                object test = base.GetFormattedValue(value, rowIndex, ref cellStyle, valueTypeConverter, formattedValueTypeConverter, context);
                return test;
            }
        }


        public override object ParseFormattedValue(object formattedValue, DataGridViewCellStyle cellStyle, TypeConverter formattedValueTypeConverter, TypeConverter valueTypeConverter)
        {
            //No formatting needed. Just return the object as is.
            return formattedValue;            
        }
    }
}
