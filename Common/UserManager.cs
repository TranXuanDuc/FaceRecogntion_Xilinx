using System;
using System.Collections.Generic;
using Microsoft.Kinect;
using Microsoft.Kinect.Wpf.Controls;
using Microsoft.Kinect.Input;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.IO;
namespace HD2
{
    class UserEventArg : EventArgs
    {

        public ulong BodyId { get; private set; }

        public UserEventArg(ulong newbody)
        {

            BodyId = newbody;
        }
    }
    static class UserManager
    {
        public static Dictionary<ulong, int> Users; //to change between program interactive states
        public static bool IsMainUserInteractive = false;
        public static Body[] bodies;
        public static ulong currentUser = 0;
        public static ulong closestUser = 0;
        public static string[] SwipeLeftArray;
        public static string[] SwipeRightArray;
        public static SwipeAction SwipeLeftAction;
        public static SwipeAction SwipeRightAction;
    }

    class UserFrameManager : IDisposable //class usermanager quan ly user
    {
        int LastFrameBodyCount;
        bool LastUserView = true;
        bool LastInteractive = false; //check last interactive state, to change program state
        KinectSensor kinectSensor;
        event EventHandler<UserEventArg> UserIn;
        event EventHandler<UserEventArg> LostUser;
        event EventHandler<UserEventArg> NewUser;
        event EventHandler<UserEventArg> NoUser;
        List<HandRaiseDetector> handRaiseList = null;
        MainWindow main;
        GestureDetector gestureDetector = null;
        HandPointer handPointer;
        UserView userView;

        System.Windows.Threading.DispatcherTimer userouttimer = new System.Windows.Threading.DispatcherTimer();
        //call in main window
        public UserFrameManager(KinectSensor sensor)
        {  //call in main window
            UserManager.Users = new Dictionary<ulong, int>();
            kinectSensor = sensor;

            userouttimer.Interval = new TimeSpan(0, ConfigParams.refreshtime, 0);
            userouttimer.Tick += userouttimer_Tick;

            //khoi tao bodyframe
            BodyFrameReader bodyFrameReader = kinectSensor.BodyFrameSource.OpenReader();
            UserManager.bodies = new Body[6];
            bodyFrameReader.FrameArrived += Reader_FrameArrived;
            FrameDescription frameDescription = kinectSensor.DepthFrameSource.FrameDescription;

            //khoi tao userview
            userView = new UserView(sensor, frameDescription.Width, frameDescription.Height);
            main = (MainWindow)App.Current.MainWindow;
            main.UserView.DataContext = userView;

            //
            UserIn += OnUserIn;
            NewUser += OnNewUser;
            LostUser += OnLostUser;
            NoUser += OnNoUser;
            int maxBodies = sensor.BodyFrameSource.BodyCount;
            handRaiseList = new List<HandRaiseDetector>(maxBodies);
            for (int i = 0; i < maxBodies; i++)
            {
                HandRaiseDetector hrd = new HandRaiseDetector();
                handRaiseList.Add(hrd);
                hrd.HandRaise += hrd_HandRaise;
            }
            gestureDetector = new GestureDetector(kinectSensor);
            handPointer = new HandPointer();
            handPointer.DisengageUser += handPointer_DisengageUser;
        }

        private void userouttimer_Tick(object sender, EventArgs e)
        {

            if (UserManager.Users.Count <= 0)
            {
                try
                {
                    using (StreamWriter sr2 = new StreamWriter("C:\\GDS_Controller\\Temp\\run.txt", false))
                    {
                        sr2.WriteLine("home");
                    }
                }

                catch (Exception)
                {

                }
            }
        }

        void handPointer_DisengageUser(object sender, PointerEventArg e)
        {
            DisengageUser(e.BodyId);
        }

        void hrd_HandRaise(object sender, HandRaiseEventArg e)
        {
            //if(UserManager.closestUser==e.bodyId){
            switch (e.Joint)
            {
                case JointType.HandLeft:
                    if (UserManager.bodies[UserManager.Users[e.bodyId]].HandLeftState == HandState.Open)
                    {
                        EngageUser(e.bodyId, e.Joint);
                        UserManager.IsMainUserInteractive = true;
                    }
                    break;
                case JointType.HandRight:
                    if (UserManager.bodies[UserManager.Users[e.bodyId]].HandRightState == HandState.Open)
                    {
                        EngageUser(e.bodyId, e.Joint);
                        UserManager.IsMainUserInteractive = true;
                    }
                    break;
            }
            //}
        }

        private void OnNoUser(object sender, UserEventArg e)
        {
            Trace.WriteLine("No User " + e.BodyId);
            DisengageUser(e.BodyId);

            StateControl.SwitchState(ProgramState.H0_G0_I0);
            try
            {
                userouttimer.Start();
            }
            catch (Exception) { }
        }

        private void OnNewUser(object sender, UserEventArg e)
        {
            //sqlAddUser(e.BodyId);
            Trace.WriteLine("New user " + e.BodyId);

        }

        private void OnLostUser(object sender, UserEventArg e)
        {
            Trace.WriteLine("Lost User" + e.BodyId);
            //sqlDeleteUser(e.BodyId);
            if (UserManager.currentUser != 0 && e.BodyId == UserManager.currentUser)
            {
                DisengageUser(e.BodyId);
            }
        }

        private void OnUserIn(object sender, UserEventArg e)
        {
            try { userouttimer.Stop(); }
            catch (Exception) { }
            Trace.WriteLine("User In " + e.BodyId);
            StateControl.SwitchState(ProgramState.H1_G0_I0);

        }

        void Reader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            //Trace.WriteLine("UserLists.Users " + UserLists.Users.Count);

            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {

                    bodyFrame.GetAndRefreshBodyData(UserManager.bodies);

                    List<ulong> trackedIds = new List<ulong>();
                    foreach (Body body in UserManager.bodies)
                    {
                        if (body == null)
                        {
                            continue;
                        }
                        if (body.IsTracked)
                        {
                            trackedIds.Add(body.TrackingId);
                        }
                    }
                    List<ulong> knownIds = new List<ulong>(UserManager.Users.Keys);
                    foreach (ulong id in knownIds)
                    {
                        if (!trackedIds.Contains(id))
                        {

                            if (LastFrameBodyCount == 1)
                            {
                                //invoke no user
                                NoUser.Invoke(null, new UserEventArg(id));

                            }
                            else
                            {
                                LostUser.Invoke(null, new UserEventArg(id));//invoke lost user

                            }
                            UserManager.Users.Remove(id);
                        }
                    }
                    for (int i = 0; i < UserManager.bodies.Length; i++)
                    {
                        if (UserManager.bodies[i] == null)
                        {
                            continue;
                        }
                        if (UserManager.bodies[i].IsTracked)
                        {

                            if (!UserManager.Users.ContainsKey(UserManager.bodies[i].TrackingId))
                            {
                                UserManager.Users[UserManager.bodies[i].TrackingId] = i;
                                if (LastFrameBodyCount == 0)
                                {
                                    //Invoke user in
                                    UserIn.Invoke(null, new UserEventArg(UserManager.bodies[i].TrackingId));
                                }
                                else
                                {
                                    //Invoke found new user
                                    NewUser.Invoke(null, new UserEventArg(UserManager.bodies[i].TrackingId));
                                }
                            }

                            //Trace.WriteLine(body.TrackingId);
                        }
                    }
                    if (UserManager.Users.Count > 0)
                    {
                        gestureDetector.IsPaused = false;
                        ulong closestUser = ChooseClosestSkeletons(UserManager.bodies, 1);
                        if (UserManager.closestUser != closestUser)
                        {
                            UserManager.closestUser = closestUser;
                        }
                        bool isInteractive = UserManager.IsMainUserInteractive;
                        if (UserManager.currentUser != gestureDetector.TrackingId)
                            gestureDetector.TrackingId = UserManager.currentUser;
                        if (isInteractive != LastInteractive)
                        {
                            if (isInteractive)
                            {
                                StateControl.SwitchState(ProgramState.H1_G1_I1);

                            }
                            else
                            {
                                StateControl.SwitchState(ProgramState.H1_G0_I0);


                            }

                            LastInteractive = isInteractive;
                        }

                        //updateinterval++;
                        //if (updateinterval >= 60)
                        //{
                        //    foreach (var bd in UserManager.bodies)
                        //    {
                        //        if (bd.TrackingId != 0)
                        //            sqlUpdateUser(bd.TrackingId);
                        //    }
                        //    updateinterval = 0;
                        //}
                    }
                    else gestureDetector.IsPaused = true;
                    int maxBodies = kinectSensor.BodyFrameSource.BodyCount;
                    for (int i = 0; i < maxBodies; i++) //control pause and resume hand raise detector
                    {
                        Body body = UserManager.bodies[i];
                        ulong trackingId = body.TrackingId;
                        if (trackingId != handRaiseList[i].trackingId)
                        {
                            handRaiseList[i].trackingId = trackingId;
                            handRaiseList[i].isPaused = trackingId == 0;
                        }
                        handRaiseList[i].Update();
                    }

                    if (UserManager.currentUser != 0 && UserManager.IsMainUserInteractive)  //move mouse control if interactive
                        handPointer.Update();
                    LastFrameBodyCount = UserManager.Users.Count;
                    if (ConfigParams.UserView != LastUserView)
                    {
                        if (ConfigParams.UserView)
                        {
                            main.UserView.Opacity = 100;
                        }
                        else
                        {
                            main.UserView.Opacity = 0;
                        }
                        LastUserView = ConfigParams.UserView;
                    }
                    if (ConfigParams.UserView)
                        userView.DrawBodies();
                }


            }
        }

        ulong ChooseClosestSkeletons(IEnumerable<Body> skeletonDataValue, int count)
        {
            SortedList<double, ulong> depthSorted = new SortedList<double, ulong>();
            try
            {
                foreach (Body s in skeletonDataValue)
                {
                    if (s.IsTracked)
                    {
                        double valueZ = Math.Sqrt(s.Joints[JointType.SpineBase].Position.Z * s.Joints[JointType.SpineBase].Position.Z + s.Joints[JointType.SpineBase].Position.X * s.Joints[JointType.SpineBase].Position.X);
                        while (depthSorted.ContainsKey(valueZ))
                        {
                            valueZ += 0.0001;
                        }

                        depthSorted.Add(valueZ, s.TrackingId);
                    }
                }

                return this.ChooseSkeletonsFromList(depthSorted.Values, count);
            }
            catch (Exception)
            {
                return 0;
            };
        }

        ulong ChooseSkeletonsFromList(IList<ulong> list, int max)
        {

            int argCount = Math.Min(list.Count, max);
            try
            {

                if (argCount == 1)
                {


                    return list[0];
                }
                return 0;
            }
            catch (Exception)
            {
                return 0;
            };

        }

        public void EngageUser(ulong user, JointType joint)
        {

            if (UserManager.currentUser == 0)
            {
                UserManager.currentUser = user;
                handPointer.InitializeMousePos(joint);
                Trace.WriteLine("engage " + user);
                //sqlUpdateGesture("active", user);
            }
            else if (UserManager.currentUser != user && !UserManager.IsMainUserInteractive)
            {
                DisengageUser(user);
                UserManager.currentUser = user;
                handPointer.InitializeMousePos(joint);
                Trace.WriteLine("change user " + user);
                //sqlUpdateGesture("active", user);
            }
        }

        void DisengageUser(ulong bodyid)
        {
            //sqlUpdateGesture("none", bodyid);
            UserManager.currentUser = 0;
            UserManager.IsMainUserInteractive = false;
        }

        public void Dispose()
        {
            //GC.SuppressFinalize(this);
        }
    }
}
