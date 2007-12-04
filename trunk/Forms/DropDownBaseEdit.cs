using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Core.Forms;
using System.Core.Helpers;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Core.Design;

namespace System.Core.Forms
{                           
    [Designer(typeof(DropDownDesigner))]    
    public abstract partial class DropDownBaseEdit : EditBase
    {
        protected readonly TextBoxEx _textBox = new TextBoxEx();
        private readonly DropDownButton _dropDownButton = new DropDownButton();
        private const int DROPDOWNBUTTON_WIDTH = 15;
        private Control _dropDownControl = null;
        private DropDownForm _dropDownForm = null;
        private bool _isDropDownVisible = false;
        private int _dropDownWidth = 0;
        private int _dropDownHeight = 200;
        private Size _lastDropDownSize = Size.Empty;        
        private bool _rememeberSize = true;


        

        public bool IsDropDownVisible
        {
            get { return _isDropDownVisible; }
        }

        [DefaultValue(true)]
        public bool RememeberSize
        {
            get { return _rememeberSize; }
            set { _rememeberSize = value; }
        }


        public int DropDownHeight
        {
            get { return _dropDownHeight; }
            set { _dropDownHeight = value; }
        }

        public int DropDownWidth
        {
            get { return _dropDownWidth; }
            set { _dropDownWidth = value; }
        }


        internal bool ProcessDropDownFormKeyStroke(Keys keyData)
        {
            return OnProcessCmdKey(keyData);
        }

        internal bool ProcessDropDownFormKeyPreview(ref Message m)
        {
            return ProcessKeyPreview(ref m);
        }



        protected override bool OnProcessCmdKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Alt | Keys.Down:
                    ShowDropDown();
                    break;
                case Keys.Escape:
                    if (_isDropDownVisible)
                    {
                        CloseDropDown(false, true);
                        return true;
                    }
                    else                        
                        return false;
                    
                case Keys.Tab:
                    CloseDropDown(false,false);
                    break;
                case Keys.Shift | Keys.Tab:
                    CloseDropDown(false,false);
                    break;
            }            
            return base.OnProcessCmdKey(keyData);
        }


        public DropDownBaseEdit()
        {
            InitializeComponent();
            this.SuspendLayout();


            //Add the controls to the controls collection
            Controls.Add(_textBox);
            Controls.Add(_dropDownButton);

            _textBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            _textBox.Location = new Point(3, 3);

            _dropDownButton.Width = DROPDOWNBUTTON_WIDTH;
            _dropDownButton.TabStop = false;

            ResumeLayout(true);
            _dropDownButton.MouseDown += (dropDownButton1_MouseDown);
                    
            
        }

                                
        

        

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            if (_textBox != null)
                _textBox.Focus();
        }

       

        void dropDownButton1_MouseDown(object sender, MouseEventArgs e)
        {
            ShowDropDown();
        }
        protected override void OnResize(EventArgs e)
        {
            _textBox.Width = Width - DROPDOWNBUTTON_WIDTH - 6;
            SetControlHeight();
            base.OnResize(e);
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            _textBox.BackColor = BackColor;
            base.OnBackColorChanged(e);
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            if (_textBox != null)
            {
                _textBox.Font = Font;
                SetControlHeight();
                
            }
        }

        

        private void SetControlHeight()
        {
            Height = _textBox.Height + 7;
            _dropDownButton.Location = new Point(Width - 17, 2);
            _dropDownButton.Height = Height - 4;
        }


        protected Control DropDownControl
        {
            get { return _dropDownControl; }
            set { _dropDownControl = value; }
        }

        public void ShowDropDown()
        {
            if (!_isDropDownVisible)
            {
                _dropDownForm = CreateDropDownForm();
                WireDropDownFormEvents();
                PositionDropDownHolder(_dropDownForm);
                _dropDownForm.Show();
                _isDropDownVisible = true;
            }
            else
                CloseDropDown(false,true);
        }

        void _dropDownForm_Deactivate(object sender, EventArgs e)
        {
            CloseDropDown(true,false);
        }

        protected virtual void OnDropDownClosed(DropDownClosedEventArgs e)
        {
            _lastDropDownSize = e.DropdownSize;
        }


        private void WireDropDownFormEvents()
        {
            _dropDownForm.Deactivate += (_dropDownForm_Deactivate);
        }

        private void UnWireDropDownFormEvents()
        {
            _dropDownForm.Deactivate -= (_dropDownForm_Deactivate);
        }



        protected virtual void OnDropDownFormCreated(DropDownForm dropDownForm)
        {
            
        }

        

        protected void CloseDropDown(bool fromDropDownWindow,bool cancelled)
        {
            if (!fromDropDownWindow || (!_dropDownButton.RectangleToScreen(_dropDownButton.ClientRectangle).Contains(MousePosition)))
            {
                if (_dropDownForm != null)
                {
                    UnWireDropDownFormEvents();
                    Size dropDownSize = _dropDownForm.Size;
                    _dropDownForm.Close();
                    _isDropDownVisible = false;
                    OnDropDownClosed(new DropDownClosedEventArgs(cancelled, dropDownSize));
                }
            }   
        }

        


        



        private DropDownForm CreateDropDownForm()
        {
            DropDownForm form = new DropDownForm(this,_dropDownControl);
            form.TopMost = true;
            form.FormBorderStyle = FormBorderStyle.None;            
            form.Owner = (Form)ControlHelper.GetParentForm(this);            
            form.StartPosition = FormStartPosition.Manual;
            form.KeyPreview = true;
            form.Size = CalulateDropDownSize(); 
            form.MinimumSize = new Size(Width,100); 
            new FocusEmulator(form.Owner);
            OnDropDownFormCreated(form);
            return form;
        }

        protected virtual Size CalulateDropDownSize()
        {
            
            
            if (_rememeberSize && _lastDropDownSize != Size.Empty)
            {
                return _lastDropDownSize;
            }
            else
            {
                return new Size(_dropDownWidth == 0 ? Width : _dropDownWidth,_dropDownHeight);
            }                                    
        }




        

        

        // Positions the drop-down holder form at the correct place.
        private void PositionDropDownHolder(Form dropDownForm)
        {
            // convert picker location to screen coordinates.
            Point loc = Parent.PointToScreen(Location);
            Screen currentScreen = Screen.FromPoint(loc);
            Rectangle screenRect = currentScreen.WorkingArea;

            // Position the dropdown X coordinate in order to be displayed in its entirely.
            if (loc.X < screenRect.X)
                loc.X = screenRect.X;
            else if ((loc.X + dropDownForm.Width) > screenRect.Right)
                loc.X = screenRect.Right - dropDownForm.Width;

            // Do the same for the Y coordinate.
            if ((loc.Y + Height + dropDownForm.Height) > screenRect.Bottom)
                loc.Offset(0, -dropDownForm.Height);  // dropdown will be above the picker control
            else
                loc.Offset(0, Height); // dropdown will be below the picker

            dropDownForm.Location = loc;
        }

       
    }
}
