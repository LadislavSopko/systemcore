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
    public partial class DropDownButton : Control
    {
        private static VisualStyleElement _currentElement = VisualStyleElement.ComboBox.DropDownButton.Normal;
        private static VisualStyleRenderer renderer = new VisualStyleRenderer(_currentElement);
        public DropDownButton()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.UserPaint,true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);            
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            if (Application.RenderWithVisualStyles)
            {
                
                
                renderer.SetParameters(_currentElement);
                
                renderer.DrawBackground(pe.Graphics, this.ClientRectangle);

            }
           

            // Calling the base class OnPaint
            base.OnPaint(pe);
        }

        protected override void OnResize(EventArgs e)
        {
            this.Invalidate();
            base.OnResize(e);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _currentElement = VisualStyleElement.ComboBox.DropDownButton.Hot;
            this.Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _currentElement = VisualStyleElement.ComboBox.DropDownButton.Normal;
            this.Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            _currentElement = VisualStyleElement.ComboBox.DropDownButton.Pressed;
            this.Invalidate();
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            _currentElement = VisualStyleElement.ComboBox.DropDownButton.Normal;
            this.Invalidate();
            base.OnMouseUp(e);
        }


        
    }
}
