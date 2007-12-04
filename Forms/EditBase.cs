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
    public enum EditBorderStyle
    {
        None = 0,
        Single = 1
    }
    
    
    
    [DefaultBindingProperty("EditValue")]
    public abstract partial class EditBase : ControlBase, ISupportInitialize 
    {
        private object _editValue;        
        private bool _isInitialized;
        private EditBorderStyle _borderStyle = EditBorderStyle.Single;

        public EditBase()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.AllPaintingInWmPaint,true);
            this.KeyPreview = true;
        }

        [DefaultValue(EditBorderStyle.Single)]
        public EditBorderStyle BorderStyle
        {
            get { return _borderStyle; }
            set
            {
                if (_borderStyle != value)
                {
                    _borderStyle = value;
                    Invalidate();
                }
            }
        }

        protected bool IsInitialized
        {
            get { return _isInitialized; }
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            VisualStyleRenderer renderer = new VisualStyleRenderer(VisualStyleElement.TextBox.TextEdit.Normal);
            pe.Graphics.FillRectangle(new SolidBrush(renderer.GetColor(ColorProperty.FillColor)), this.ClientRectangle);
            if (_borderStyle == EditBorderStyle.Single)
            {
                Rectangle borderRect = this.ClientRectangle;
                borderRect.Width -= 1;
                borderRect.Height -= 1;
                pe.Graphics.DrawRectangle(new Pen(renderer.GetColor(ColorProperty.BorderColor)), borderRect);
            }
            base.OnPaint(pe);
        }

        

        
        

        

       


        protected override bool OnProcessCmdKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Shift | Keys.Tab:
                    this.FindForm().SelectNextControl(this, false, false, false, true);
                    return true;

                case Keys.Tab:
                    this.FindForm().SelectNextControl(this, true, false, false, true);
                    return true;
                default:
                    return false;
            }            
        }

        
        [Bindable(true)]
        public object EditValue
        {
            get { return _editValue; }
            set
            {
                _editValue = value;
                OnEditValueChanged();
            }
        }


        protected virtual void OnEditValueChanged()
        {
            
        }

        protected virtual void OnNewEditValue()
        {
            
        }

        #region ISupportInitialize Members

        void ISupportInitialize.BeginInit()
        {
            _isInitialized = false;
            OnBeginInit();
        }

        void ISupportInitialize.EndInit()
        {
            _isInitialized = true;
            OnEndInit();            
        }

        protected virtual void OnBeginInit()
        {
            
        }

        protected virtual void OnEndInit()
        {
            
        }




     
       


        #endregion

        
    }
}
