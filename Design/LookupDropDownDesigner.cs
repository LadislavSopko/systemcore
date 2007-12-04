using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Text;

namespace System.Core.Design
{
    public class LookupDropDownDesigner : DropDownDesigner
    {
        DesignerActionListCollection _actionLists = null;
        
        public override DesignerActionListCollection ActionLists
        {
            get
            {
                if (_actionLists == null)
                {
                    _actionLists = new DesignerActionListCollection();
                    _actionLists.Add(new LookupDropDownActionList(this));
                }
                return _actionLists;
            }
        }



        
    }
}
