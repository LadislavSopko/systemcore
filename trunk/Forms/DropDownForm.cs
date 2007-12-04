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
    public partial class DropDownForm : Form
    {
        private const int CS_DROPSHADOW = 0x20000;
        private Rectangle _sizeGripRectangle;
        FormFader _formFader;
        DropDownBaseEdit _owner;
        Control _dropDownControl;
        public DropDownForm()
        {
            InitializeComponent();
            
        }


        


        protected override void OnResize(EventArgs e)
        {
            CalculateSizeGripRectangle();
            this.Invalidate();
            base.OnResize(e);
        }

        private void CalculateSizeGripRectangle()
        {
            _sizeGripRectangle.X = this.ClientRectangle.Width - 20;
            _sizeGripRectangle.Y = this.ClientRectangle.Height - 20;
            _sizeGripRectangle.Width = 16;
            _sizeGripRectangle.Height = 16;
        }


       

        protected override CreateParams CreateParams
        {
            get
            {
                //Apply the dropshadow style is OS version is XP or later
                CreateParams createParams = base.CreateParams;
                if (Environment.OSVersion.Version.Major >= 5)
                    createParams.ClassStyle |= CS_DROPSHADOW;
                return createParams;
            }
        }

        public DropDownForm(DropDownBaseEdit owner, Control dropDownControl)
        {
            InitializeComponent();
            _formFader = new FormFader(this, 5, 20);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            _owner = owner;
            if (dropDownControl != null)
            {
                _dropDownControl = dropDownControl;
                this.Controls.Add(dropDownControl);
                this.DockPadding.All = 1;
                this.DockPadding.Bottom = 20;
                dropDownControl.Dock = DockStyle.Fill;
                dropDownControl.Visible = true;
            }
           
        }

        protected override void OnPaint(PaintEventArgs e)
        {          
            DrawBorder(e.Graphics); 
            base.OnPaint(e);
        }
        
        /// <summary>
        /// Draws the border for the dropdown form and the sizegrip in the lower right corner
        /// </summary>
        /// <param name="g"></param>
        private void DrawBorder(Graphics g)
        {
            VisualStyleRenderer renderer = new VisualStyleRenderer(VisualStyleElement.TextBox.TextEdit.Normal);
            Rectangle borderRect = this.ClientRectangle;
            borderRect.Width -= 1;
            borderRect.Height -= 1;
            g.DrawRectangle(new Pen(renderer.GetColor(ColorProperty.BorderColor)), borderRect);
            renderer = new VisualStyleRenderer(VisualStyleElement.Status.Gripper.Normal);
            renderer.DrawBackground(g, _sizeGripRectangle);
        }

       

        protected override void WndProc(ref Message m)
        {
            // Trap WM_NCHITTEST
            if (m.Msg == 0x84)
            {


                // Check if mouse close to lower-right corner
                Point pos = this.PointToClient(new Point(m.LParam.ToInt32() & 0xffff, m.LParam.ToInt32() >> 16));
                if (pos.X >= this.Width - 16 && pos.Y >= this.Height - 16)
                {
                    m.Result = (IntPtr)17;  // HTBOTTOMRIGHT
                    return;
                }
            }
            base.WndProc(ref m);

        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {

            const int WM_KEYDOWN = 0x100;
            const int WM_SYSKEYDOWN = 0x104;

            if ((msg.Msg == WM_KEYDOWN) || (msg.Msg == WM_SYSKEYDOWN))            
                return _owner.ProcessDropDownFormKeyStroke(keyData);                                
            else
                return base.ProcessCmdKey(ref msg, keyData);
        }


        

        protected override void OnClosing(CancelEventArgs e)
        {
            //Make sure that we clear the controls collection.
            //Otherwise the controls contained will be disposed
            this.Controls.Clear();
            base.OnClosing(e);
        }

        protected override bool ProcessKeyPreview(ref Message m)
        {
            return _owner.ProcessDropDownFormKeyPreview(ref m);
        }

        private void DropDownForm_Load(object sender, EventArgs e)
        {

        }
     
    }
}