
using System;
using System.Collections.Generic;
using Microsoft.Kinect;
using Microsoft.Kinect.VisualGestureBuilder;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System.Windows.Input;
namespace HD2
{
    enum GestureType
    {
        LeftToRight,
        RightToLeft,
    }
    public class GestureDetector : IDisposable
    {
        /// <summary> Path to the gesture database that was trained with VGB </summary>
        Gesture LeftToRight;
        Gesture RightToLeft;
        Gesture clapGesture;
        Gesture ZoomOut;
        int gestureResetTime = 300;
        bool EnableSwipe = true;
        bool EnableZoom = true;
        bool EnableClap = true;
        private const string VGB_DATABASE_FILE = @"GestureBuilder\ClappingHands.gbd";
        const string ZoomOutdtb = @"GestureBuilder\ZoomOut.gbd";
        const string Swipedtb = @"GestureBuilder\Swipe.gbd";
        /// <summary> Gesture frame source which should be tied to a body tracking ID </summary>
        private VisualGestureBuilderFrameSource vgbFrameSource = null;
        MainWindow main;
        /// <summary> Gesture frame reader which will handle gesture events coming from the sensor </summary>
        private VisualGestureBuilderFrameReader vgbFrameReader = null;

        public GestureDetector(KinectSensor kinectSensor)
        {
            if (kinectSensor == null)
            {
                return;
            }
            main = (MainWindow)App.Current.MainWindow;
            // create the vgb source. The associated body tracking ID will be set when a valid body frame arrives from the sensor.
            this.vgbFrameSource = new VisualGestureBuilderFrameSource(kinectSensor, 0);

            // open the reader for the vgb frames
            this.vgbFrameReader = this.vgbFrameSource.OpenReader();
            if (this.vgbFrameReader != null)
            {
                this.vgbFrameReader.IsPaused = true;
                this.vgbFrameReader.FrameArrived += this.Reader_GestureFrameArrived;
            }

            using (VisualGestureBuilderDatabase database = new VisualGestureBuilderDatabase(VGB_DATABASE_FILE))
            {


                foreach (Gesture gesture in database.AvailableGestures)
                {

                    this.vgbFrameSource.AddGesture(gesture);
                    clapGesture = gesture;
                }
            }
            using (VisualGestureBuilderDatabase database = new VisualGestureBuilderDatabase(ZoomOutdtb))
            {
                foreach (Gesture gesture in database.AvailableGestures)
                {
                    vgbFrameSource.AddGesture(gesture);
                    ZoomOut = gesture;
                }
            }
            using (VisualGestureBuilderDatabase database = new VisualGestureBuilderDatabase(Swipedtb))
            {
                foreach (Gesture gesture in database.AvailableGestures)
                {
                    vgbFrameSource.AddGesture(gesture);
                    if (gesture.Name == "Swipe_Right")
                    {
                        RightToLeft = gesture;

                    }
                    if (gesture.Name == "Swipe_Left")
                    {
                        LeftToRight = gesture;

                    }
                }
            }
        }

        public ulong TrackingId
        {
            get
            {

                return this.vgbFrameSource.TrackingId;
            }

            set
            {
                if (this.vgbFrameSource.TrackingId != value)
                {
                    this.vgbFrameSource.TrackingId = value;
                }
            }
        }

        public bool IsPaused
        {
            get
            {
                return this.vgbFrameReader.IsPaused;
            }

            set
            {
                if (this.vgbFrameReader.IsPaused != value)
                {
                    this.vgbFrameReader.IsPaused = value;
                }
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.vgbFrameReader != null)
                {
                    this.vgbFrameReader.FrameArrived -= this.Reader_GestureFrameArrived;
                    this.vgbFrameReader.Dispose();
                    this.vgbFrameReader = null;
                }

                if (this.vgbFrameSource != null)
                {
                    this.vgbFrameSource.Dispose();
                    this.vgbFrameSource = null;
                }
            }
        }
        private void Reader_GestureFrameArrived(object sender, VisualGestureBuilderFrameArrivedEventArgs e)
        {
            VisualGestureBuilderFrameReference frameReference = e.FrameReference;
            using (VisualGestureBuilderFrame frame = frameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    IReadOnlyDictionary<Gesture, DiscreteGestureResult> discreteResults = frame.DiscreteGestureResults;
                    IReadOnlyDictionary<Gesture, ContinuousGestureResult> continuousResults =
                        frame.ContinuousGestureResults;
                    if (discreteResults != null)
                    {
                        DiscreteGestureResult SwipeFromLeftResult = null;
                        DiscreteGestureResult SwipeFromRightResult = null;
                        DiscreteGestureResult ZoomOutResult = null;

                        discreteResults.TryGetValue(ZoomOut, out ZoomOutResult);
                        discreteResults.TryGetValue(RightToLeft, out SwipeFromRightResult);
                        discreteResults.TryGetValue(LeftToRight, out SwipeFromLeftResult);
                        if (EnableSwipe && SwipeFromRightResult != null && SwipeFromRightResult.Detected && SwipeFromRightResult.Confidence > 0.35)
                        {
                            Trace.WriteLine("Swipe from right");
                            OnSwipe(GestureType.RightToLeft);
                            EnableSwipe = false;
                            ResetSwipe(gestureResetTime);
                        }
                        if (EnableSwipe && SwipeFromLeftResult != null && SwipeFromLeftResult.Detected && SwipeFromLeftResult.Confidence > 0.35)
                        {
                            Trace.WriteLine("swipe from left");
                            OnSwipe(GestureType.LeftToRight);
                            EnableSwipe = false;
                            ResetSwipe(gestureResetTime);
                        }


                        if (EnableZoom && ZoomOutResult != null && ZoomOutResult.Detected && ZoomOutResult.Confidence > 0.5)
                        {
                            OnZoomOut();
                        }

                    }
                    if (continuousResults != null)
                    {
                        ContinuousGestureResult ClapResult = null;
                        continuousResults.TryGetValue(clapGesture, out ClapResult);
                        if (EnableClap && ClapResult != null)
                        {
                            float clapProg = ClapResult.Progress;
                            if (clapProg > 1.85 && clapProg < 3)
                            {
                                Trace.WriteLine("Clap detected");
                                EnableClap = false;
                                ResetClap(gestureResetTime);
                            }
                        }

                    }
                }
            }
        }
        void OnSwipe(GestureType CurrentGesture)
        {
            switch (CurrentGesture)
            {
                case GestureType.LeftToRight:
                    switch (UserManager.SwipeLeftAction)
                    {
                        case SwipeAction.sendkey:
                            sendKey(UserManager.SwipeLeftArray[2]);
                            Trace.WriteLine("sendkey " + UserManager.SwipeLeftArray[2]);
                            break;
                        case SwipeAction.click:

                            int xpos = 0; int ypos = 0;
                            GetMousePos(out xpos, out ypos); //save last mouse pos
                            uint clickX = (uint)Int16.Parse(UserManager.SwipeLeftArray[2]);
                            uint clickY = (uint)Int16.Parse(UserManager.SwipeLeftArray[3]);
                            NativeMethods.SetCursorPos((int)clickX, (int)clickY); //move to click position
                            DoMouseClick(clickX, clickY); //click
                            NativeMethods.SetCursorPos(xpos, ypos); //return to previous position
                            Trace.WriteLine("click " + clickX + " " + clickY);
                            break;
                        default:
                            break;
                    }
                    break;
                case GestureType.RightToLeft:
                    switch (UserManager.SwipeRightAction)
                    {
                        case SwipeAction.sendkey:
                            sendKey(UserManager.SwipeRightArray[2]);
                            Trace.WriteLine("sendkey " + UserManager.SwipeRightArray[2]);
                            break;
                        case SwipeAction.click:
                            int xpos = 0; int ypos = 0;
                            GetMousePos(out xpos, out ypos); //save last mouse pos
                            uint clickX = (uint)Int16.Parse(UserManager.SwipeRightArray[2]);
                            uint clickY = (uint)Int16.Parse(UserManager.SwipeRightArray[3]);
                            NativeMethods.SetCursorPos((int)clickX, (int)clickY); //move to click position
                            DoMouseClick(clickX, clickY); //click
                            NativeMethods.SetCursorPos(xpos, ypos); //return to previous position
                            Trace.WriteLine("click " + clickX + " " + clickY);
                            break;
                        default:
                            break;
                    }
                    break;
            }
        }
        void DoMouseClick(uint xpos, uint ypos)
        {
            NativeMethods.mouse_event(NativeMethods.MOUSEEVENTF_LEFTDOWN | NativeMethods.MOUSEEVENTF_LEFTUP, xpos, ypos, 0, 0);
        }

        //Get current mouse position
        void GetMousePos(out int xpos, out int ypos)
        {
            main.CaptureMouse();
            var Position = Mouse.GetPosition(main);
            xpos = (int)Position.X;
            ypos = (int)Position.Y;
            main.ReleaseMouseCapture();
        }

        //On zoom out event
        void OnZoomOut()
        {
            Trace.WriteLine("Zoom Out");
            //Sound alert
            StateInstruction.PlaySound(AssetSource.beep);
            //==============================================================

            //Write action to run.txt file
            try
            {
                using (StreamWriter sr2 = new StreamWriter("C:\\GDS_Controller\\GUI_Controller\\Launcher.DesktopApp.txt", false))
                {
                    sr2.WriteLine("home");
                }
            }
            catch (Exception e) { Trace.WriteLine(e.ToString()); }
            //==============================================================

            //Reset timer
            EnableZoom = false;
            ResetZoom(gestureResetTime);
        }
        async void ResetZoom(int time)
        {
            await Task.Delay(time);
            EnableZoom = true;
        }
        async void ResetSwipe(int time)
        {
            await Task.Delay(time);
            EnableSwipe = true;
        }
        async void ResetClap(int time)
        {
            await Task.Delay(time);
            EnableClap = true;
        }
        private void sendKey(string key)
        {

            Process[] p = Process.GetProcessesByName("POWERPNT");
            Trace.WriteLine(p.Count());
            if (p.Count() > 0)
            {
                IntPtr h = p[0].MainWindowHandle;
                NativeMethods.SetForegroundWindow(h);

            }

            System.Windows.Forms.SendKeys.SendWait(key);
            if (p.Count() > 0)
            {
                IntPtr h = p[0].MainWindowHandle;
                NativeMethods.SetForegroundWindow(h);
                System.Threading.Thread.Sleep(300);
            }

        }
    }
}
