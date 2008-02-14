using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace System.Common.Forms
{
    //[ToolboxItem(false)]
    public partial class TextBoxEx : TextBox
    {

        [DllImport("user32.dll")]

        private static extern int SetCaretPos(int x, int y);


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

        
        public void SetCaretPosition(Point location)
        {
            SetCaretPos(location.X, location.Y);
        }

        public override bool PreProcessMessage(ref Message msg)
        {
            return base.PreProcessMessage(ref msg);
        }


        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
