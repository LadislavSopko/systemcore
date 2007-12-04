using System;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows.Forms;
using Timer = System.Timers.Timer;


namespace System.Core.Forms
{
    /// <summary>
    /// Adds fade transitions to a standard form. 
    /// </summary>
    public class FormFader : NativeWindow
    {

        #region Private Member Variables

        /// <summary>
        /// Reference to the form being faded
        /// </summary>
        private readonly Form _targetForm;

        /// <summary>
        /// Represents the default number of frames/steps the transition should span. 
        /// </summary>
        private readonly byte _frames = 5;

        /// <summary>
        /// The total duration of the fade transition.
        /// </summary>
        private readonly int _duration = 10;

        /// <summary>
        /// The current opacity/alpha value.
        /// </summary>
        private byte _currentAlpha = 0;

        /// <summary>
        /// The times that actually performs the fade.
        /// </summary>
        private readonly Timer _timer;

        /// <summary>
        /// Represents the opacity/alpha value to increase/decrease for each frame. 
        /// </summary>
        private readonly byte _alphaStep;

        /// <summary>
        /// Indicates if fading transition is currently running. 
        /// </summary>
        private bool _isFading = false;

        /// <summary>
        /// Represents the type of fade transition
        /// </summary>
        private enum FadeOperation
        {
            /// <summary>
            /// The form fades into view
            /// </summary>
            FadeIn,
            /// <summary>
            /// The form fades out of view
            /// </summary>
            FadeOut
        }

        /// <summary>
        /// The <see cref="FadeOperation"/> currently pending/running.
        /// </summary>
        private FadeOperation _fadeOperation = FadeOperation.FadeIn;




        #endregion

        #region External Methods

        /// <summary>
        /// The SetLayeredWindowAttributes function sets the opacity and transparency color key of a layered window.
        /// </summary>        
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        /// <summary>
        /// This function changes an attribute of the specified window
        /// </summary>        
        [DllImport("user32.dll", SetLastError = true)]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        /// <summary>
        /// The GetWindowLong function retrieves information about the specified window. 
        /// </summary>
        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        /// <summary>
        /// The SendMessage function sends the specified message to a window or windows. 
        /// It calls the window procedure for the specified window and does not return until the window procedure has processed the message. 
        /// </summary>        
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SendMessage(HandleRef hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        #endregion

        #region Constants

        /// <summary>
        /// Used in GetWindowLong and SetWindowLong to get/set the extended style of the target form.
        /// </summary>
        private const int GWL_EXSTYLE = (-20);

        /// <summary>
        /// Applies the layered style to the target forms extended style
        /// </summary>
        private const int WS_EX_LAYERED = 0x00080000;

        /// <summary>
        /// Tells SetLayeredWindowAttributes to use the argument bAlpha to determine the opacity of the layered window.
        /// </summary>
        private const int LWA_ALPHA = 0x2;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="FormFader"/> class
        /// </summary>
        /// <param name="targetForm">The target form that the fade effect will be applied to</param>
        public FormFader(Form targetForm)
            : this(targetForm, 5, 10)
        {

        }

        /// <summary>
        /// Creates a new instance of the <see cref="FormFader"/> class
        /// </summary>
        /// <param name="targetForm">The target form that the fade effect will be applied to</param>
        /// <param name="frames">The number of frames to be used in the fade transition.</param>
        /// <param name="duration">The number of milliseconds for the fade transition</param>
        public FormFader(Form targetForm, byte frames, int duration)
        {
            _frames = frames;
            _duration = duration;

            targetForm.HandleCreated += (targetForm_HandleCreated);
            targetForm.HandleDestroyed += (targetForm_HandleDestroyed);
            targetForm.FormClosing += (targetForm_FormClosing);
            targetForm.Load += (targetForm_Load);
            _targetForm = targetForm;

            _timer = new Timer(_duration / _frames);
            //Set the SynchronizingObject to avoid cross-thread invocation  
            _timer.SynchronizingObject = _targetForm;
            _timer.Elapsed += (_timer_Elapsed);

            //Calculate the opacity increase/decrease amount for each frame            
            _alphaStep = (byte)(255 / _frames);
            
        }

       

        void targetForm_Load(object sender, EventArgs e)
        {
            //Check to see if the form is about to be shown
          
                //Make sure that we are dealing with a layered window
                EnsureLayeredWindow();
                //Set the fade oparetion                      
                _fadeOperation = FadeOperation.FadeIn;
                //Start the transition
                _timer.Enabled = true;
          
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Sets the opacity/aplha value for the <see cref="_targetForm"/>.
        /// </summary>
        /// <param name="opacity"></param>
        private void SetOpacity(byte opacity)
        {
            SetLayeredWindowAttributes(_targetForm.Handle, 0, opacity, LWA_ALPHA);            
        }

        /// <summary>
        /// Ensures that we are dealing with a real layered window.
        /// </summary>
        private void EnsureLayeredWindow()
        {
            int exStyle = GetWindowLong(Handle, GWL_EXSTYLE);
            if (!((exStyle & WS_EX_LAYERED) == WS_EX_LAYERED))
            {
                SetWindowLong(Handle, GWL_EXSTYLE, (IntPtr)(exStyle ^ WS_EX_LAYERED));
            }
        }

        void targetForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!e.Cancel && e.CloseReason == CloseReason.UserClosing)
            {
                //Make sure that we are not already in a fading process.
                //This can happen if the fade in operation is not finished before the user closes the form
                if (_isFading)
                {
                    _timer.Enabled = false;
                    e.Cancel = false;
                }
                else
                {
                    //Set the fade operation
                    _fadeOperation = FadeOperation.FadeOut;
                    //Start the transition
                    _timer.Enabled = true;

                    e.Cancel = true;
                }
            }
        }

        void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {


            if (_fadeOperation == FadeOperation.FadeIn)
            {
                _isFading = true;
                if (_currentAlpha + _alphaStep >= 255)
                {
                    _currentAlpha = 255;
                    _timer.Enabled = false;
                    _isFading = false;
                }
                else
                    _currentAlpha += _alphaStep;

                SetOpacity(_currentAlpha);
            }
            if (_fadeOperation == FadeOperation.FadeOut)
            {

                _isFading = true;
                if (_currentAlpha - _alphaStep <= 0)
                {
                    _currentAlpha = 0;
                    _timer.Enabled = false;
                    _targetForm.Close();
                    _isFading = false;
                }
                else
                {
                    _currentAlpha -= _alphaStep;
                    SetOpacity(_currentAlpha);
                }
            }
        }


        /// <summary>
        /// Occurs when the control's handle is in the process of being destroyed. 
        /// </summary>        
        void targetForm_HandleDestroyed(object sender, EventArgs e)
        {
            _timer.Stop();
            ReleaseHandle();
        }

        /// <summary>
        /// Occurs when a handle is created for the control. 
        /// </summary>        
        void targetForm_HandleCreated(object sender, EventArgs e)
        {
            AssignHandle(((Form)sender).Handle);
        }

        #endregion

        

    }
}
