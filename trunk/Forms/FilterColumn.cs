using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace System.Common.Forms
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

        [TypeConverter("System.Common.Design.LookupDropDownDataMemberConverter,System.Common.Design.dll")]
        public string DataField
        {
            get { return _dataField; }
            set { _dataField = value; }
        }
    }
}
