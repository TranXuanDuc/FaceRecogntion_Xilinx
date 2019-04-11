using Microsoft.Kinect;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System;
using Microsoft.Kinect.Input;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
namespace HD2
{
    class PointerEventArg : EventArgs
    {
        public ulong BodyId { get; private set; }
        public PointerEventArg(ulong bodyid)
        {
            BodyId = bodyid;
        }
    }
    class HandPointer
    {
        Stopwatch stopWatch;
        ulong user;
        public int scaleX = 3000; // chuyen sang nhap tu config
        public int scaleY = 3500;
        int ClickTime = 50; //min click time in milliseconds
        int HoldTime = 500; //min hold time in milliseconds
        event EventHandler MouseDownHandler;
        event EventHandler MouseUpHandler;
        event EventHandler HoldingEvent;
        public event EventHandler<PointerEventArg> DisengageUser;
        MainWindow main;

        double X;
        double Y;

        List<double> xHis;
        List<double> yHis;

        double historyLength = 5; //chong nhieu
        double currentIndex = 0; //index de chong nhieu

        bool MouseDown = false;
        float yDistance = 0.3f;
        double thresold = 0.3;
        public JointType currentJoint;

        double lastMouseX;
        double lastMouseY;

        bool IsHolding = false;
        GridRegion gridRegion;

        public HandPointer()
        {
            xHis = new List<double>();
            yHis = new List<double>();

            stopWatch = new Stopwatch(); //timer control hold and click
            scaleX = Int32.Parse(ConfigParams.arrConfig[(int)configList.ScaleX]);
            scaleY = Int32.Parse(ConfigParams.arrConfig[(int)configList.ScaleY]);
            main = (MainWindow)App.Current.MainWindow;
            ClickTime = Int32.Parse(ConfigParams.arrConfig[(int)configList.TimerClick]);
            HoldTime = Int32.Parse(ConfigParams.arrConfig[(int)configList.TimerHold]);

            MouseDownHandler += OnMouseDown;
            MouseUpHandler += OnMouseUp;
            HoldingEvent += OnHolding;
            gridRegion = new GridRegion();
        }

        //Declear function for simulate mouse move
        [DllImport("User32.dll",
           EntryPoint = "mouse_event",
           CallingConvention = CallingConvention.Winapi)]
        internal static extern void Mouse_Event(int dwFlags,
                                                int dx,
                                                int dy,
                                                int dwData,
                                                int dwExtraInfo);

        [DllImport("User32.dll",
                   EntryPoint = "GetSystemMetrics",
                   CallingConvention = CallingConvention.Winapi)]
        internal static extern int InternalGetSystemMetrics(int value);
        //End simulation declear
        public void Update()//update mouse cursor
        {
            user = UserManager.currentUser;
            //get hand pos
            Vector3D handpos = GetVector3FromCameraSpacePoint(UserManager.bodies[UserManager.Users[user]].Joints[currentJoint].Position);
            //get wrist pos
            Vector3D wristPos = GetVector3FromCameraSpacePoint(UserManager.bodies[UserManager.Users[user]].Joints[currentJoint - 1].Position);
            double ydelta = handpos.Y - wristPos.Y;
            //calculate mouse pos based on handpos and wrist pos
            Vector3D mousePos = new Vector3D(handpos.X, handpos.Y + (thresold - ydelta), handpos.Z);
            //calculate difference between current pos and last pos
            double xDeltaHand = mousePos.X - lastMouseX;
            double yDeltaHand = mousePos.Y - lastMouseY;

            double scaledX = xDeltaHand * scaleX;
            double scaledY = yDeltaHand * scaleY;
            //chong nhieu
            if (currentIndex < historyLength)
            {

                xHis.Add(scaledX);
                yHis.Add(scaledY);
                currentIndex++;
            }
            else
            {
                for (int i = 0; i < historyLength - 1; i++)
                {
                    xHis[i] = xHis[i + 1];
                    yHis[i] = yHis[i + 1];
                }
                xHis[(int)historyLength - 1] = scaledX;
                yHis[(int)historyLength - 1] = scaledY;
            }
            //lay trung binh cua cac frame truoc
            double currentX = xHis.Average();
            double currentY = yHis.Average();
            //toa do chuot can di chuyen
            X += currentX;
            Y -= currentY;
            //Trace.WriteLine(X + " " + Y);

            // Move mouse cursor to an absolute position to_x, to_y
            int to_x;
            int to_y;

            int screenWidth = InternalGetSystemMetrics(0);
            int screenHeight = InternalGetSystemMetrics(1);

            if (ConfigParams.EnableGrid)
            {
                int xIndex = ((int)X / gridRegion.pixel_x);
                int yIndex = ((int)Y / gridRegion.pixel_y);
                MatchgridRegion();  //doc cac file xml
                try
                {
                    if (xIndex >= 0 && yIndex >= 0)
                    {
                        if (gridRegion.arrayXmouseDefault[gridRegion.arrayGridRegion[yIndex, xIndex]] != 0
        && gridRegion.arrayYmouseDefault[gridRegion.arrayGridRegion[yIndex, xIndex]] != 0)
                        {
                            to_x = gridRegion.arrayXmouseDefault[gridRegion.arrayGridRegion[yIndex, xIndex]];
                            to_y = gridRegion.arrayYmouseDefault[gridRegion.arrayGridRegion[yIndex, xIndex]];
                            // Mickey X coordinate
                            int mic_x = (int)System.Math.Round(to_x * 65536.0 / screenWidth);
                            // Mickey Y coordinate
                            int mic_y = (int)System.Math.Round(to_y * 65536.0 / screenHeight);
                            //NativeMethods.SetCursorPos(gridRegion.arrayXmouseDefault[gridRegion.arrayGridRegion[yIndex, xIndex]], gridRegion.arrayYmouseDefault[gridRegion.arrayGridRegion[yIndex, xIndex]]);
                            // 0x0001 | 0x8000: Move + Absolute position
                            Mouse_Event(0x0001 | 0x8000, mic_x, mic_y, 0, 0);
                        }
                        else
                        {
                            // Mickey X coordinate
                            int mic_x = (int)System.Math.Round((int)X * 65536.0 / screenWidth);
                            // Mickey Y coordinate
                            int mic_y = (int)System.Math.Round((int)Y * 65536.0 / screenHeight);
                            // 0x0001 | 0x8000: Move + Absolute position
                            Mouse_Event(0x0001 | 0x8000, mic_x, mic_y, 0, 0);
                            //NativeMethods.SetCursorPos((int)X, (int)Y);
                        }
                    }
                }
                catch (IndexOutOfRangeException ex)
                {
                    Trace.WriteLine(ex.ToString());
                }

            }
            else
            {
                // Mickey X coordinate
                int mic_x = (int)System.Math.Round((int)X * 65536.0 / screenWidth);
                // Mickey Y coordinate
                int mic_y = (int)System.Math.Round((int)Y * 65536.0 / screenHeight);
                // 0x0001 | 0x8000: Move + Absolute position
                Mouse_Event(0x0001 | 0x8000, mic_x, mic_y, 0, 0);
                //NativeMethods.SetCursorPos((int)X, (int)Y);
            }

            switch (currentJoint)
            {
                case JointType.HandLeft:

                    if (UserManager.bodies[UserManager.Users[user]].HandLeftState == HandState.Closed)
                    {
                        MouseDownHandler(null, EventArgs.Empty);

                    }
                    else if (MouseDown)
                    {
                        MouseUpHandler(null, EventArgs.Empty);
                    }

                    break;
                case JointType.HandRight:
                    if (UserManager.bodies[UserManager.Users[user]].HandRightState == HandState.Closed)
                    {
                        MouseDownHandler(null, EventArgs.Empty);
                    }
                    else if (MouseDown)
                    {
                        MouseUpHandler(null, EventArgs.Empty);
                    }

                    break;
            }
            SaveLastHandPos(currentJoint);
            float distance = (float)handpos.Y - UserManager.bodies[UserManager.Users[user]].Joints[JointType.SpineBase].Position.Y;
            //Trace.WriteLine(distance);
            if (distance < yDistance)
            {
                Trace.WriteLine("Disengage " + user);
                DisengageUser(null, new PointerEventArg(user));
                //disengage user if not interact
            }

        }
        private void OnHolding(object sender, EventArgs e)
        {
            //Trace.WriteLine("on holding " + ConfigParams.EnableGrid + " " + gridRegion.enHold);
            if (!ConfigParams.EnableGrid)
            {
                int xpos; int ypos;
                GetMousePos(out xpos, out ypos);
                //Trace.WriteLine("not enable grid " + xpos + " " + ypos);
                NativeMethods.mouse_event(NativeMethods.MOUSEEVENTF_LEFTDOWN, (uint)xpos, (uint)ypos, 0, 0);
            }
            else if (gridRegion.enHold)
            {
                int xpos; int ypos;
                GetMousePos(out xpos, out ypos);
                //Trace.WriteLine("enable hold " + xpos + " " + ypos);
                NativeMethods.mouse_event(NativeMethods.MOUSEEVENTF_LEFTDOWN, (uint)xpos, (uint)ypos, 0, 0);
            }

        }
        void MatchgridRegion()
        {

            string path = gridRegion.readWebPage();


            gridRegion.readActiveRegion(path);
            gridRegion.matchActiveRegion();

        }

        private void OnMouseUp(object sender, EventArgs e)
        {
            if (MouseDown)
            {
                MouseDown = false;
                //Trace.WriteLine("mouse up");
            }
            long elapsedmilli = stopWatch.ElapsedMilliseconds;
            //check tay trong trang thai click chuot
            if (elapsedmilli > ClickTime && elapsedmilli < HoldTime)
            {

                NativeMethods.mouse_event(NativeMethods.MOUSEEVENTF_LEFTDOWN | NativeMethods.MOUSEEVENTF_LEFTUP, (uint)X, (uint)Y, 0, 0); //click chuot
            }
            else if (elapsedmilli >= HoldTime) //dong tac di chuot
            {
                int xpos; int ypos;
                GetMousePos(out xpos, out ypos);

                NativeMethods.mouse_event(NativeMethods.MOUSEEVENTF_LEFTUP, (uint)xpos, (uint)ypos, 0, 0); //nha chuot

            }
            IsHolding = false;
            StopTimer();
        }

        private void OnMouseDown(object sender, EventArgs e)
        {

            if (!MouseDown)
            {
                //Trace.WriteLine("mouse down");
                MouseDown = true;

                StartTimer();
            }
            else
            {
                long elapsedMilli = stopWatch.ElapsedMilliseconds;
                if (elapsedMilli > HoldTime) // thoi gian hold => cho vao config
                {
                    if (!IsHolding)
                    {
                        HoldingEvent(null, EventArgs.Empty);

                        IsHolding = true;
                    }

                }
            }
        }
        void StartTimer()
        {
            stopWatch.Start();

        }
        void StopTimer()
        {
            stopWatch.Reset();

        }
        void SaveLastHandPos(JointType CurrentHand) //calculate mouse pos and save
        {
            Vector3D lastHand = GetVector3FromCameraSpacePoint(UserManager.bodies[UserManager.Users[user]].Joints[CurrentHand].Position);
            Vector3D lastWrist = GetVector3FromCameraSpacePoint(UserManager.bodies[UserManager.Users[user]].Joints[CurrentHand - 1].Position);
            double Ydifference = lastHand.Y - lastWrist.Y;
            lastMouseX = lastHand.X;
            lastMouseY = lastHand.Y + (thresold - Ydifference);

        }
        public void InitializeMousePos(JointType currentHand) //initialize cursor position at the center of the screen
        {
            currentIndex = 0;
            xHis.Clear();
            yHis.Clear();
            X = System.Windows.SystemParameters.PrimaryScreenWidth / 2;
            Y = System.Windows.SystemParameters.PrimaryScreenHeight / 2;
            user = UserManager.currentUser;
            currentJoint = currentHand;

            SaveLastHandPos(currentHand);
        }
        Vector3D GetVector3FromCameraSpacePoint(CameraSpacePoint point)
        {
            Vector3D result = new Vector3D();
            result.X = point.X;
            result.Y = point.Y;
            result.Z = point.Z;
            return result;
        }
        void GetMousePos(out int xpos, out int ypos)
        {
            main.CaptureMouse();
            var Position = Mouse.GetPosition(main);
            xpos = (int)Position.X;
            ypos = (int)Position.Y;
            main.ReleaseMouseCapture();
        }

    }
}
