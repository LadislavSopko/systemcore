using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Common.Forms;
using System.Drawing.Design;
using System.Text;

namespace System.Common.Design
{
    public class LookupDropDownActionList : DesignerActionList
    {
        LookupDropDownDesigner _designer;
        LookupDropDown _lookupDropDown;

        public LookupDropDownActionList(LookupDropDownDesigner designer) : base(designer.Component)
        {
            _designer = designer;
            _lookupDropDown = (LookupDropDown)designer.Component;
        }

        public override DesignerActionItemCollection GetSortedActionItems()
        {
            DesignerActionItemCollection designerActions = new DesignerActionItemCollection();
            designerActions.Add(new DesignerActionHeaderItem("Binding Options"));
            designerActions.Add(new DesignerActionPropertyItem("DataSource", "Data Source", "Binding Options","The data source used for the items in the dropdown"));
            designerActions.Add(new DesignerActionPropertyItem("DisplayMember", "Display Member", "Binding Options","The name of the property that is displayed in the dropdown editbox."));
            designerActions.Add(new DesignerActionMethodItem(this, "RetrieveColumns", "Retrieve columns from datasource", "Columns", "Retrieves the columns to be used in the dropdown from the current datasource."));
            return designerActions;
        }

        [AttributeProvider(typeof(IListSource))]
        public object DataSource
        {
            get { return _lookupDropDown.DataSource; }
            set { _lookupDropDown.DataSource = value; }
        }
        
        [Editor("System.Windows.Forms.Design.DataMemberFieldEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string DisplayMember
        {
            get { return _lookupDropDown.DisplayMember; }
            set { _lookupDropDown.DisplayMember = value; }
        }

        public void RetrieveColumns()
        {
            _lookupDropDown.RetrieveColumns();
        }

    }
}
