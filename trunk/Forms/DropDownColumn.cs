using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Text;
using System.Windows.Forms;

namespace System.Common.Forms
{
    [Serializable]
    public class DropDownColumn : DataColumn
    {
        private string _name;
        
        private int _fillWeight = 100;


        public DropDownColumn()
        {
        }

        public DropDownColumn(string dataField) : base(dataField)
        {
            _name = dataField;
        }

        public DropDownColumn(string name, string dataField) :base(dataField)
        {
            _name = name;            
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        
        public int FillWeight
        {
            get { return _fillWeight; }
            set { _fillWeight = value; }
        }
    }
}
