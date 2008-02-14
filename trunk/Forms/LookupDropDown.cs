using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Common.Design;
using System.Data;
using System.Drawing;
using System.Drawing.Design;
using System.Text;
using System.Windows.Forms;
using System.Common.Emit;
using System.Common.Collections;

namespace System.Common.Forms
{        
    [Designer(typeof(LookupDropDownDesigner))]
    public partial class LookupDropDown : DropDownBaseEdit
    {
        private readonly DataGridView _dataGrid = new DataGridView();
        private string _displayMember;
        private List<DropDownColumn> _columns = new List<DropDownColumn>();
        private List<DataColumn> _filterColumns = new List<DataColumn>();
        private bool _filterAllColumns = true;
        private int _rowHeight = 20;

        private CurrencyManager _currencyManager;
        
        private StringBuilder _filterBuffer = new StringBuilder();
        private Type _listItemType = null;

        private TypedArrayList _dropDownDataSource;
        private object _dataSource;
        private bool _isFiltered;

        
        
        private PropertyDescriptorCollection _propertyDescriptors;



        public LookupDropDown()
        {
            InitializeComponent();

            _dataGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
            _dataGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _dataGrid.MultiSelect = false;
            _dataGrid.AutoGenerateColumns = true;
            _dataGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _dataGrid.RowTemplate.Height = 20;
            _dataGrid.RowHeadersVisible = false;
            _dataGrid.BackgroundColor = _dataGrid.DefaultCellStyle.BackColor;
            _dataGrid.AllowUserToDeleteRows = false;
            _dataGrid.AllowUserToAddRows = false;
            _dataGrid.AllowUserToResizeRows = false;
            _dataGrid.ReadOnly = true;

            
            this.DropDownControl = _dataGrid;

            _dataGrid.SelectionChanged += new EventHandler(_dataGrid_SelectionChanged);                
        }

        /// <summary>
        /// Sets or gets the row height in the dropdown.
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(20)]
        [Description("The height of the data rows in the dropdown")]       
        public int RowHeight
        {
            get { return _rowHeight; }
            set { _rowHeight = value; }
        }


      

        [Category("Data")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]                
        public List<DataColumn> FilterColumns
        {
            get { return _filterColumns; }
            set { _filterColumns = value; }
        }

        /// <summary>
        /// Determines if filtering should be performed on all available columns 
        /// </summary>
        public bool FilterAllColumns
        {
            get { return _filterAllColumns; }
            set { _filterAllColumns = value; }
        }

        protected override void OnDropDownFormCreated(DropDownForm dropDownForm)
        {
            if (_currencyManager != null)
            {
                Find(EditValue);
                dropDownForm.BindingContext[_dataGrid.DataSource].Position = _currencyManager.Position;
            }
            base.OnDropDownFormCreated(dropDownForm);
        }

        


      

        private void CreateColumns()
        {
            if (!this.DesignMode)
            {
                if (_columns.Count == 0)
                {
                    foreach (PropertyDescriptor propertyDescriptor in _propertyDescriptors)
                    {
                        _columns.Add(new DropDownColumn(propertyDescriptor.Name));
                    }
                }
                                                
                if (_columns.Count > 0)
                {
                    _dataGrid.Columns.Clear();
                    foreach (DropDownColumn dropDownColumn in _columns)
                    {
                        DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
                        column.HeaderText = dropDownColumn.Name;
                        column.Name = dropDownColumn.Name;
                        column.DataPropertyName = dropDownColumn.DataField;
                        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                        column.FillWeight = dropDownColumn.FillWeight;
                        _dataGrid.Columns.Add(column);                       
                        OnColumnCreated(column);
                    }
                }

                if (_filterAllColumns)
                {
                    foreach (DropDownColumn dropDowncolumn in _columns)
                    {
                        _filterColumns.Add(dropDowncolumn);
                    }
                }

            }
        }


        private ICollection FilterCollection(ICollection sourceList,string propertyName,object value)
        {
            ArrayList filteredList = new ArrayList();
            foreach (object item in sourceList)
            {
                if (_propertyDescriptors[propertyName].GetValue(item).ToString().StartsWith(value.ToString(),StringComparison.InvariantCultureIgnoreCase))
                {
                    filteredList.Add(item);
                }
            }

            return filteredList;
        }





        protected void OnColumnCreated(DataGridViewColumn column)
        {
            //column.DefaultCellStyle.BackColor = Color.DodgerBlue;
        }


        [Category("Data")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]                
        public List<DropDownColumn> Columns
        {
            get { return _columns; }           
        }

        

        

        protected override bool OnProcessCmdKey(Keys keyData)
        {
                                    


            if (keyData == Keys.Enter)
            {
                if (IsDropDownVisible)
                {
                    CloseDropDown(false,false);
                    return true;
                }
            }
            if (keyData == Keys.Back)
            {
                if (_filterBuffer.Length > 0)
                {
                    _filterBuffer.Length -= 1;
                    UpdateFilter();
                    
                }
                return true;
            }
            
            

            return base.OnProcessCmdKey(keyData);

        }

        protected override void OnDropDownClosed(DropDownClosedEventArgs e)
        {
            
            if (!e.Cancelled )
            {
                if (_dataGrid.SelectedRows.Count > 0)
                {                    
                    EditValue = _dataGrid.SelectedRows[0].DataBoundItem; 
                    OnNewEditValue();                   
                }
            }
            else
            {
                UpdateTextFromEditValue();
            }



            base.OnDropDownClosed(e);
            RemoveFilter();
        }


        private void RemoveFilter()
        {
            _filterBuffer.Length = 0;
            UpdateFilter();
        }


        private void UpdateFilter()
        {
            bool found = false;
            if (_filterBuffer.Length == 0)
            {
                _dataGrid.DataSource = _dropDownDataSource;
                _textBox.HideSelection = true;
                _isFiltered = false;
            }
            else
            {
                _isFiltered = true;
                _textBox.HideSelection = false;
                ICollection filteredList;
                foreach (DataColumn column in _filterColumns)
                {
                    filteredList = FilterCollection(_dropDownDataSource, column.DataField, _filterBuffer);
                    if (filteredList.Count > 0)
                    {
                        _dataGrid.DataSource = new TypedArrayList(filteredList, _propertyDescriptors);
                        found = true;

                        _textBox.Text = _propertyDescriptors[column.DataField].GetValue(_dataGrid.SelectedRows[0].DataBoundItem).ToString();
                        _textBox.SelectionLength = _filterBuffer.Length;

                        break;
                    }
                }

                if (!found)
                    _filterBuffer.Length -= 1;






            }         
        }





        private void Find(object item)
        {
            if (_currencyManager != null)
            {
                if (item != null && item.ToString() == string.Empty)
                    _currencyManager.Position = -1;
                else

                _currencyManager.Position = _currencyManager.List.IndexOf(item);                 
            }

        }

        private void UpdateTextFromEditValue()
        {
            if (EditValue != null && EditValue.ToString() != string.Empty)
            {
                _textBox.Text = GetDisplayValue(EditValue);                    
            }
            else
                _textBox.Text = "";

        }
       
        public string GetDisplayValue(object item)
        {
            if (_displayMember == null || _displayMember == string.Empty)
                return item.ToString();
            else
                return _propertyDescriptors[_displayMember].GetValue(item).ToString();
        }


        protected override void OnEditValueChanged()
        {
            UpdateTextFromEditValue();                  
        }


        void _dataGrid_SelectionChanged(object sender, EventArgs e)
        {
            if (IsDropDownVisible && !_isFiltered)
            {
                if (_dataGrid.SelectedRows.Count > 0)
                {
                    _textBox.Text = GetDisplayValue(_dataGrid.SelectedRows[0].DataBoundItem);                        
                }
                else
                    _textBox.Text = "";
            }
        }

        [Category("Data")]
        [AttributeProvider(typeof(IListSource))]
        public object DataSource
        {
            get { return _dataSource; }
            set
            {
                _dataSource = value;
                //if (!this.DesignMode && IsInitialized)
                //{                                        
                //    ResolveRuntimeDataSource();                                                       
                //}
                if (!this.DesignMode)
                {
                    ResolveRuntimeDataSource();
                }
            }
        }


        private void ResolveRuntimeDataSource()
        {
         
            if (_dataSource == null | _dataSource is Type) return;
                        
            //Check to see if the datasource is set to a BindingSource instance
            if (typeof(BindingSource).IsAssignableFrom(_dataSource.GetType()))
            {
                ((BindingSource)_dataSource).DataSourceChanged += (bs_DataSourceChanged);
                ((BindingSource)_dataSource).DataMemberChanged += (bs_DataMemberChanged);                    
            }            

            if (typeof(ICollection).IsAssignableFrom(_dataSource.GetType()))            
                CreateDropDownDataSource((ICollection)_dataSource);
            else
                throw new InvalidOperationException("The datasource has to implement ICollection as a minimum implementation" );

            CreateColumns();
        }

        private void CreateDropDownDataSource(ICollection collection)
        {
            _listItemType = ListBindingHelper.GetListItemType(collection);
            
            if (typeof(ITypedList).IsAssignableFrom(collection.GetType()))
            {
                _propertyDescriptors = ((ITypedList)collection).GetItemProperties(null);                
            }
            else
            {                
                if (!IsGenericCollection(collection.GetType()) && collection.Count == 0)
                    throw new InvalidOperationException("Unable to determine listitem type");
                else
                _propertyDescriptors = TypeDescriptor.GetProperties(_listItemType);

            }
            
            _dropDownDataSource = new TypedArrayList(collection,_propertyDescriptors);
            _dataGrid.DataSource = _dropDownDataSource;

            if (BindingContext != null)
            _currencyManager = (CurrencyManager)BindingContext[_dataGrid.DataSource];
        }


        



        private void bs_DataMemberChanged(object sender, EventArgs e)
        {
            ResolveRuntimeDataSource();
            
        }


        private bool IsGenericList (Type type)
		{			            
            if (type.IsGenericType && type.GetGenericTypeDefinition () == typeof (IList<>))
				return true;
			foreach (Type i in type.GetInterfaces ())
				if (IsGenericList (i))
					return true;
			return false;
		}

        private bool IsGenericCollection(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ICollection<>))
                return true;
            foreach (Type i in type.GetInterfaces())
                if (IsGenericCollection(i))
                    return true;
            return false;
        }

       

        




        private void bs_DataSourceChanged(object sender, EventArgs e)
        {
            ResolveRuntimeDataSource();                                                
        }

        

        

        

        [Category("Data")]
        [Editor("System.Windows.Forms.Design.DataMemberFieldEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string DisplayMember
        {
            get { return _displayMember; }
            set { _displayMember = value; }
        }

        



        public void RetrieveColumns()
        {
            _columns.Clear();
            if (DataSource == null) return;
            ITypedList listSource = (DataSource as ITypedList);
            if (listSource == null) return;

            PropertyDescriptorCollection properties = listSource.GetItemProperties(null);

            foreach(PropertyDescriptor property in properties)
            {
                _columns.Add(new DropDownColumn(property.Name,property.Name));
            }
        }


        protected override void OnEndInit()
        {
            if (DesignMode) return;
            
            ResolveRuntimeDataSource();
            _textBox.HideSelection = false;
            _textBox.ReadOnly = true;
            base.OnEndInit();
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if ((ModifierKeys & Keys.Control) != Keys.Control)
            {
                if (!IsDropDownVisible)
                {
                    ShowDropDown();
                }

                _filterBuffer.Append(e.KeyChar);
                UpdateFilter();
                e.Handled = true;
            }
            base.OnKeyPress(e);
        }

      
 }
}
