using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace System.Core.Forms
{
    [Serializable]
    public class DataColumn
    {
        private string _dataField;


        public DataColumn()
        {
        }

        public DataColumn(string dataField)
        {
            _dataField = dataField;
        }

        [TypeConverter("System.Core.Design.LookupDropDownDataMemberConverter,System.Core.Design.dll")]
        public string DataField
        {
            get { return _dataField; }
            set { _dataField = value; }
        }
    }
}
