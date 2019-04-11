using Microsoft.Kinect;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System;
using System.Windows.Media.Media3D;
namespace HD2
{
    class HandRaiseEventArg : EventArgs
    {
        public JointType Joint { get; private set; }
        public ulong bodyId { get; private set; }
        public HandRaiseEventArg(JointType joint, ulong newbody)
        {
            Joint = joint;
            bodyId = newbody;
        }
    }
    class HandRaiseDetector
    {
        int AngleThresold = 50;
        public event EventHandler<HandRaiseEventArg> HandRaise;
        ulong UserId;
        public ulong trackingId = 0;
        public bool isPaused = true;
        long RaiseTime = 200;
        bool countLeft = true;
        bool countRight = true;
        Stopwatch stopwatchLeft;
        Stopwatch stopwatchRight;

        //Create stopwatch object for left and right hand
        public HandRaiseDetector()
        {
            stopwatchLeft = new Stopwatch();
            stopwatchRight = new Stopwatch();
        }

        //Update position of the raised hand
        public void Update()
        {
            if (trackingId != 0)
            {
                UserId = trackingId;
            }
            if (!isPaused)
            {
                //Get hand, elbow position of left and right hand with tracking ID from kinect camera
                #region GetPosition
                CameraSpacePoint leftHandPos = UserManager.bodies[UserManager.Users[trackingId]].Joints[JointType.HandLeft].Position;
                CameraSpacePoint rightHandPos = UserManager.bodies[UserManager.Users[trackingId]].Joints[JointType.HandRight].Position;
                CameraSpacePoint leftEl = UserManager.bodies[UserManager.Users[trackingId]].Joints[JointType.ElbowLeft].Position;
                CameraSpacePoint rightEl = UserManager.bodies[UserManager.Users[trackingId]].Joints[JointType.ElbowRight].Position;
                #endregion
                //=================================================================================

                //Check left hand rise
                #region CheckLeftHand
                if (CheckHandRaise(leftHandPos, leftEl))
                {
                    if (countLeft)
                    {
                        stopwatchLeft.Start();
                        countLeft = false;
                    }
                    long leftElapsed = stopwatchLeft.ElapsedMilliseconds;
                    if (leftElapsed > RaiseTime)
                    {
                        OnHandRaise(JointType.HandLeft, UserId);
                        ResetCountLeft();
                    }
                }
                else
                {
                    ResetCountLeft();
                }
                #endregion
                //=================================================================================

                //Check right hand rise
                #region CheckRightHand
                if (CheckHandRaise(rightHandPos, rightEl))
                {
                    if (countRight)
                    {
                        stopwatchRight.Start();
                        countRight = false;
                    }
                    long rightElapsed = stopwatchRight.ElapsedMilliseconds;
                    if (rightElapsed > RaiseTime)
                    {
                        OnHandRaise(JointType.HandRight, UserId);
                        ResetCountRight();
                    }
                }
                else
                {
                    ResetCountRight();

                }
                #endregion
                //=================================================================================
            }
        }

        //Reset counter for left hand
        void ResetCountLeft()
        {
            countLeft = true;
            stopwatchLeft.Reset();
        }

        //Reset counter for right hand
        void ResetCountRight()
        {
            countRight = true;
            stopwatchRight.Reset();
        }

        //Check if hand is rasing or not: if angle between arm direction and torso direction is smaller than the threshold angle
        bool CheckHandRaise(CameraSpacePoint handPosition, CameraSpacePoint elbowPosition)
        {
            Joint torso = UserManager.bodies[UserManager.Users[trackingId]].Joints[JointType.SpineMid];
            Joint head = UserManager.bodies[UserManager.Users[trackingId]].Joints[JointType.Head];
            Vector3D handPos = GetVector3FromCameraSpacePoint(handPosition);
            Vector3D elbPos = GetVector3FromCameraSpacePoint(elbowPosition);
            Vector3D headPos = GetVector3FromCameraSpacePoint(head.Position);
            Vector3D torsoPos = GetVector3FromCameraSpacePoint(torso.Position);
            Vector3D armDirection = handPos - elbPos;
            Vector3D torsoDirection = headPos - torsoPos;
            double angle = Vector3D.AngleBetween(armDirection, torsoDirection);
            return (angle < AngleThresold);
        }

        //On hand raise event
        void OnHandRaise(JointType joint, ulong id)
        {
            HandRaise.Invoke(this, new HandRaiseEventArg(joint, id));
        }

        //Get coordinates of point from kinect camera
        Vector3D GetVector3FromCameraSpacePoint(CameraSpacePoint point)
        {
            Vector3D result = new Vector3D();
            result.X = point.X;
            result.Y = point.Y;
            result.Z = point.Z;
            return result;
        }
    }
}
