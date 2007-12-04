using System.Core.Helpers;
using System.Windows.Forms;

namespace System.Core.Forms
{
    /// <summary>
    /// Base class for implementing user controls.
    /// </summary>
    /// <remarks>
    /// The purpose of this class is to support a keypreview property on the control level.
    /// This is very useful when implementing composite controls, that is controls that hosts one 
    /// or more child controls and you want to intercept key events on the top control level.
    /// </remarks>
    public partial class ControlBase : Control
    {

        #region Private Member Variables

        private bool _keyPreview = true;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="ControlBase"/> class.
        /// </summary>
        public ControlBase()
        {
            InitializeComponent();
        }

        #endregion

        #region Public Properties


        /// <summary>
        /// Gets or sets a value indicating whether the control will receive key events before the event is passed to the child control that has focus. 
        /// </summary>
        public bool KeyPreview
        {
            get { return _keyPreview; }
            set { _keyPreview = value; }
        }

        /// <summary>
        /// Returns the parent form for this control.
        /// </summary>
        public Form ParentForm
        {
            get { return (Form)ControlHelper.GetParentForm(this);}            
        }

        #endregion

        #region Protected Methods

        ///<summary>
        ///Previews a keyboard message.
        ///</summary>
        ///
        ///<returns>
        ///true if the message was processed by the control; otherwise, false.
        ///</returns>
        ///
        ///<param name="m">A <see cref="T:System.Windows.Forms.Message"></see>, passed by reference, that represents the window message to process. </param>
        protected override bool ProcessKeyPreview(ref Message m)
        {
            return (((_keyPreview) && ProcessKeyEventArgs(ref m)) || base.ProcessKeyPreview(ref m));
        }

        ///<summary>
        ///Processes a command key.
        ///</summary>
        ///
        ///<returns>
        ///true if the character was processed by the control; otherwise, false.
        ///</returns>
        ///
        ///<param name="keyData">One of the <see cref="T:System.Windows.Forms.Keys"></see> values that represents the key to process. </param>
        ///<param name="msg">A <see cref="T:System.Windows.Forms.Message"></see>, passed by reference, that represents the window message to process. </param>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            const int WM_KEYDOWN = 0x100;
            const int WM_SYSKEYDOWN = 0x104;

            if ((msg.Msg == WM_KEYDOWN) || (msg.Msg == WM_SYSKEYDOWN))
                return OnProcessCmdKey(keyData);
            else
                return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>
        /// Processes a command key.
        /// </summary>
        /// <param name="keyData">One of the <see cref="T:System.Windows.Forms.Keys"></see> values that represents the key to process.</param>
        /// <returns>true if the character was processed by the control; otherwise, false.</returns>
        protected virtual bool OnProcessCmdKey(Keys keyData)
        {
            return false;
        }

        #endregion

    }
}
