using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms.Design;
namespace System.Common.Design
{
    public class DropDownDesigner : ControlDesigner
    {
        public override SelectionRules SelectionRules
        {
                        
            
            get
            {
                SelectionRules rules = base.SelectionRules;
                rules = rules & ~System.Windows.Forms.Design.SelectionRules.BottomSizeable;
                rules = rules & ~System.Windows.Forms.Design.SelectionRules.TopSizeable;
                return rules;
            }
        }
    }
}
