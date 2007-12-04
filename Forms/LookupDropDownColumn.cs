using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace System.Core.Forms
{
    
    public class LookupDropDownColumn : DataGridViewColumn
    {

        private object _dataSource;
        
        private Size _lastDropDownSize = Size.Empty;
        
        private LookupDropDownEditingControl _lookupControl = new LookupDropDownEditingControl();

        public LookupDropDownColumn() : base(new LookupDropDownCell())
        {
            
        }


        [TypeConverter(typeof(ExpandableObjectConverter))]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public LookupDropDownEditingControl LookupControl
        {
            get { return _lookupControl; }
            set
            {
                _lookupControl = value;                
            }
        }

        public override object Clone()
        {
            LookupDropDownColumn col = base.Clone() as LookupDropDownColumn;
            col.DataSource = _dataSource;
            col.LookupControl = _lookupControl;
            col.LastDropDownSize = _lastDropDownSize;
        
            return col;

        }


        [AttributeProvider(typeof(IListSource))]
        public object DataSource
        {
            get { return _dataSource; }
            set
            {
                _dataSource = value;
                ((LookupDropDownCell)CellTemplate).DataSource = value;
            }
        }


        public Size LastDropDownSize
        {
            get { return _lastDropDownSize; }
            set { _lastDropDownSize = value; }
        }

        public override DataGridViewCell CellTemplate
        {
            get
            {
                return base.CellTemplate;
            }
            set
            {
                // Ensure that the cell used for the template is a CalendarCell.
                if (value != null &&
                    !value.GetType().IsAssignableFrom(typeof(LookupDropDownCell)))
                {
                    throw new InvalidCastException("Must be a LookupDropDownCell");
                }
                base.CellTemplate = value;

                
            }

            
        }


        
        


        
    }
}
