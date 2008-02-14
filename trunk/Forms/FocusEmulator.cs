using System.Windows.Forms;

namespace System.Common.Forms
{
    /// <summary>
    /// This class makes sure that the title bar stays "active" even if the form looses focus.
    /// </summary>
    public class FocusEmulator : NativeWindow
    {

        private const int WM_NCACTIVATE = 0x0086;

        [Runtime.InteropServices.DllImport("USER32.DLL")]
        private extern static int SendMessage(IntPtr handle, int msg, int wParam, IntPtr lParam);

        /// <summary>
        /// The form that will be the target for focus emulation
        /// </summary>
        /// <param name="form"></param>
        public FocusEmulator(IWin32Window form)
        {
            AssignHandle(form.Handle);
        }

        /// <summary>
        /// Catches the WM_NCACTIVATE message and tells the window to reactive
        /// </summary>
        /// <param name="m">The window message</param>
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_NCACTIVATE)
            {
                // Check if the title bar will made inactive:
                if (((int)m.WParam) == 0)
                {
                    // If so reactivate it.
                    SendMessage(Handle, WM_NCACTIVATE, 1, IntPtr.Zero);
                }
            }
        }
    }

}
