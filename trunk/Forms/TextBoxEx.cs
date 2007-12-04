using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace System.Core.Forms
{
    [ToolboxItem(false)]
    public partial class TextBoxEx : TextBox
    {                
        public TextBoxEx()
        {
            InitializeComponent();
        }

        protected override void OnReadOnlyChanged(EventArgs e)
        {            
            if (this.ReadOnly)
            {
                VisualStyleRenderer renderer = new VisualStyleRenderer(VisualStyleElement.TextBox.TextEdit.Normal);
                this.BackColor = renderer.GetColor(ColorProperty.FillColor);
            }                
            base.OnReadOnlyChanged(e);
        }
    }
}
