using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
namespace System.Core.Forms
{
    [Serializable]
    public class LookupDropDownEditingControl : LookupDropDown, IDataGridViewEditingControl
    {

        DataGridView _dataGridView;
        int _rowIndex;
        bool _valueChanged;


        public LookupDropDownEditingControl()
        {
            this.BorderStyle = EditBorderStyle.None;
        }


        protected override bool OnProcessCmdKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Shift | Keys.Tab:
                case Keys.Tab:
                    if (IsDropDownVisible)
                        CloseDropDown(false,true);
                    return false;
                
                default:
                    return base.OnProcessCmdKey(keyData);
            }
        }


        private LookupDropDownColumn GetCurrentColumn()
        {            
            return (LookupDropDownColumn)_dataGridView.Columns[_dataGridView.CurrentCell.ColumnIndex];
        }


        protected override void OnDropDownClosed(DropDownClosedEventArgs e)
        {
            LookupDropDownColumn column = GetCurrentColumn();
            if (column != null)
            {
                column.LastDropDownSize = e.DropdownSize;
            }
            

            base.OnDropDownClosed(e);
        }

        protected override Size CalulateDropDownSize()
        {
            LookupDropDownColumn column = GetCurrentColumn();
            if (column != null && column.LastDropDownSize != Size.Empty)
                return column.LastDropDownSize;
            else
                return base.CalulateDropDownSize();
        }



        #region IDataGridViewEditingControl Members

        public void ApplyCellStyleToEditingControl(DataGridViewCellStyle dataGridViewCellStyle)
        {
            //throw new Exception("The method or operation is not implemented.");
        }

        public DataGridView EditingControlDataGridView
        {
            get
            {
                return _dataGridView;
            }
            set
            {
                _dataGridView = value;
            }
        }

        public object EditingControlFormattedValue
        {
            get
            {
                return EditValue;                
            }
            set
            {
                EditValue = value;
            }
        }

        public int EditingControlRowIndex
        {
            get
            {
                return _rowIndex;
            }
            set
            {
                _rowIndex = value;
            }
        }

        public bool EditingControlValueChanged
        {
            get
            {
                return _valueChanged;
            }
            set
            {
                _valueChanged = value;
            }
        }

        public bool EditingControlWantsInputKey(Keys keyData, bool dataGridViewWantsInputKey)
        {
            if (ModifierKeys == Keys.Alt)
            {

                if (Keys.Down == (keyData & Keys.Down))
                    return true;
            }

            if (keyData == Keys.Escape)
                return true;
            else
                return true;
        }

        public Cursor EditingPanelCursor
        {
            get
            {
                return base.Cursor;
            }
        }

        public object GetEditingControlFormattedValue(DataGridViewDataErrorContexts context)
        {
            return EditingControlFormattedValue;
        }

        public void PrepareEditingControlForEdit(bool selectAll)
        {
            //throw new Exception("The method or operation is not implemented.");
        }

        public bool RepositionEditingControlOnValueChange
        {
            get { return false; }
        }

        protected override void OnNewEditValue()
        {
            _valueChanged = true;
            EditingControlDataGridView.NotifyCurrentCellDirty(true);
            base.OnNewEditValue();
        }
        

        #endregion
    }
}
