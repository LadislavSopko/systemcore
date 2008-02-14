using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace System.Common.Helpers
{
    /// <summary>
    /// Contains helper functions related to controls
    /// </summary>
    public static class ControlHelper
    {                        
        /// <summary>
        /// Finds the form containing the given control.
        /// </summary>
        /// <param name="ctl">The control whose parent is to be found.</param>
        /// <returns>The parent form containing the control.</returns>
        public static Control GetParentForm(Control ctl)
        {
            // Using an infinite loop, we can keep looking until a parent form is found.
            // When a parent for is fount, return causes am 'emergency exit'.
            while (true)
            {
                // If control has no parent, it is the form we are searching for.
                if (ctl.Parent == null)
                    return ctl;
                // Parent form not found - proceed to the parent of the current control
                else
                    ctl = ctl.Parent;
            }
        }   
    }
}
