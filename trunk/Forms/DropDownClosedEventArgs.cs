using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace System.Core.Forms
{
    public class DropDownClosedEventArgs : EventArgs
    {
        private bool _cancelled;
        private Size _dropdownSize;

        public DropDownClosedEventArgs(bool cancelled,Size dropdownSize)
        {
            _cancelled = cancelled;
            _dropdownSize = dropdownSize;
        }

        public bool Cancelled
        {
            get { return _cancelled; }
            set { _cancelled = value; }
        }


        public Size DropdownSize
        {
            get { return _dropdownSize; }
            set { _dropdownSize = value; }
        }
    }
}
