/*
 * Kinect controlled Pong game
 * by Craig Chao, KUAS, Taiwan, R.O.C.
 * Spring 2012
 * All rights reserved, all non-permited copies are prohibited.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.Kinect.Nui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;


/*
 * 
 JointID Member name Description 
 AnkleLeft Left ankle. 
 AnkleRight Right ankle. 
 Count Number of joints, which is used as an index to terminate a loop, not as an identifier. 
 ElbowLeft Left elbow. 
 ElbowRight Right elbow. 
 FootLeft Left foot. 
 FootRight Right foot. 
 HandLeft Left hand. 
 HandRight Right hand. 
 Head Head. 
 HipCenter Center, between hips. 
 HipLeft Left hip. 
 HipRight Right hip. 
 KneeLeft Left knee. 
 KneeRight Right knee. 
 ShoulderCenter Center, between shoulders. 
 ShoulderLeft Left shoulder. 
 ShoulderRight Right shoulder. 
 Spine Spine. 
 WristLeft Left wrist. 
 WristRight Right wrist. 

 * 
 * 
 */


namespace Cray.KinectTrack
{
    public class KinectTracker
    {

        private DebugMsg debugMsg = new DebugMsg();


        //Kinect specified
        const int TOTAL_JOINTS_NUM = (int)JointID.Count;
        Runtime nui;
        public Boolean isKinectDeviceReady = false;
        public JointsCollection Joints=null;

        const double minDistance = 0.8;
        const double maxDistance = 2.5;

        //current translated position
        private Vector[] JointPosition3  = new Vector[TOTAL_JOINTS_NUM];
        private Point[]   JointPosition2 = new Point  [TOTAL_JOINTS_NUM];
        private Boolean[] JointTracked   = new Boolean[TOTAL_JOINTS_NUM];


        //Kinect event handler
        public delegate void PositionChangedEventHandler(object sender, JointID id);
        private PositionChangedEventHandler[] handlerClientPositionChanged;
        
        //states of this kinect game
        public enum GamingStates { PaddleBall, KinectPaddleBall, KinectMan, KinectLightSaber, KinectLightsaberFight };

        public GamingStates GamingState
        {
            set;
            get;
        }


        //constructor
        public KinectTracker()
        {

            handlerClientPositionChanged = new PositionChangedEventHandler[TOTAL_JOINTS_NUM];

            try
            {
                nui = Runtime.Kinects[0];
            }
            catch
            {
                isKinectDeviceReady = false;
                Console.WriteLine("Kinect device error!");
                return;
            }

            isKinectDeviceReady = true;
            
        }

        public bool isKinectTracking
        {
            set;
            get;
        }

        public Vector2 KinectScreenSize
        {
            set;
            get;
        }


        public Vector GetJointPosition3(JointID id)
        {
            return (JointPosition3[(int)id]);
        }

        public Point GetJointPosition2(JointID id)
        {
            return (JointPosition2[(int)id]);
        }


        public Boolean isJointTracked(JointID id)
        {
            return (JointTracked[(int)id]);
        }

        public void SetKinectPositionChangedHandler(JointID id, PositionChangedEventHandler handler)
        {
            handlerClientPositionChanged[(int)id] = handler;
        }

        //real handler to do things, to be called when posision changed
        protected void OnPositionChanged(JointID id)
        {
            debugMsg.WriteLine("OnHeadPositionChanged()...id(" + (int)id + "), ("+ JointPosition3[(int)id] + ")"
                , DebugMsg.OnPositionChanged);

            PositionChangedEventHandler handler = handlerClientPositionChanged[(int)id];
            if (handler != null)
            {
                handler(this, id);
            }
        }

        public void SetJointTracked(JointID id)
        {
            JointTracked[(int)id] = true;
        }

        public void UnsetJointTracked(JointID id)
        {
            JointTracked[(int)id] = false;
        }



        public void StartTracking()
        {
            nui_Initialization();
        }


        //adapted from Kinect demo app, i.e. ShapeGame
        private Point getDisplayPosition(Joint joint)
        {
            float depthX, depthY;
            nui.SkeletonEngine.SkeletonToDepthImage(joint.Position, out depthX, out depthY);
            depthX = depthX * 320; //convert to 320, 240 space
            depthY = depthY * 240; //convert to 320, 240 space
            int colorX, colorY;
            ImageViewArea iv = new ImageViewArea();
            // only ImageResolution.Resolution640x480 is supported at this point
            nui.NuiCamera.GetColorPixelCoordinatesFromDepthPixel(ImageResolution.Resolution640x480, iv, (int)depthX, (int)depthY, (short)0, out colorX, out colorY);

            // map back to skeleton.Width & skeleton.Height
            Point p = new Point((int)(KinectScreenSize.X * colorX / 640.0), (int)(KinectScreenSize.Y * colorY / 480));
            debugMsg.WriteLine("getDisplayPosition=" + p, DebugMsg.getDisplayPosition);

            return p;
        }


        private void nui_Initialization()
        {

            debugMsg.Write("nui_Initialization()...", DebugMsg.nui_Initialization);

            if (!isKinectDeviceReady) return;

            //clear all defalt arrays
            Array.Clear(handlerClientPositionChanged, 0, TOTAL_JOINTS_NUM);
            Array.Clear(JointTracked, 0, TOTAL_JOINTS_NUM);
            isKinectTracking = false;

            try
            {
                nui.Initialize(RuntimeOptions.UseSkeletalTracking);

                nui.SkeletonFrameReady += 
                    new EventHandler<SkeletonFrameReadyEventArgs> (nui_SkeletonFrameReady);

                nui.SkeletonEngine.TransformSmooth = true;

                var parameters = new TransformSmoothParameters
                {
                    Smoothing = 0.75f, 
                    Correction = 0.5f, 
                    Prediction = 0.0f, 
                    JitterRadius = 0.50f, 
                    MaxDeviationRadius = 0.05f 
                };

                nui.SkeletonEngine.SmoothParameters = parameters;                
            }
            catch (InvalidOperationException ex)
            {
                //failed to initalize kinect
                throw ex;
            }

            
            debugMsg.WriteLine("OK!", DebugMsg.nui_Initialization);
        }

        public float RotationRadius(Point startpos, Point endpos)
        {
            int startX = startpos.X, startY = startpos.Y;
            int endX = endpos.X, endY = endpos.Y;
            int quadrant = 0;

            /*
             *   Rotate Radius:
             *                 PI
             *                 |
             *    PI/2 --------+---------- -PI/2
             *                 |
             *                 0
             * 
             */


            //special cases
            if (startX == endX) return 0;
            if (startY == endY) return (float)(Math.PI / 2.0);

            float rotation;
            float rotation0 = (float)Math.Atan((float)(endY - startY) / (float)(endX - startX));


            if (endX > startX)  //1st or 4th quadrant
            {
                if (endY < startY) //1st quadrant,  0 ~ -PI/2  ==>  -PI/2 ~ -PI
                {
                    quadrant = 1;
                    rotation = rotation0 - (float)(Math.PI / 2.0);
                }
                else //4th quadrant, PI/2 ~ 0 ==> 0 ~ -PI/2
                {
                    quadrant = 4;
                    rotation = rotation0 - (float)(Math.PI / 2.0);
                }
            }
            else  //2nd or 3rd quadrant
            {
                if (endY < startY) //2nd quadrant, 0 ~ PI/2 ==> PI/2 ~ PI
                {
                    quadrant = 2;
                    rotation = rotation0 + (float)(Math.PI / 2.0);
                }
                else //3rd quadrant, 0 ~ -PI/2 ==> PI/2 ~ 0
                {
                    quadrant = 3;
                    rotation = (float)(Math.PI / 2.0) + rotation0;
                }
            }

            debugMsg.WriteLine("Q=[ " + quadrant + ", " + rotation0 + " ] " +
                startpos + ", " + endpos + " ==> " + rotation, DebugMsg.RotationRadius);

            return (rotation);
        }


        void nui_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {


            debugMsg.Write("nui_SkeletonFrameReady()...", DebugMsg.nui_SkeletonFrameReady);

            
            foreach (var skeleton in e.SkeletonFrame.Skeletons)
            {
                if (skeleton.TrackingState != SkeletonTrackingState.Tracked)
                {
                    isKinectTracking = false;
                    continue;
                }

                isKinectTracking = true;
                //in tracked state
                Joints = skeleton.Joints;
                foreach (Joint joint in Joints)
                {
                    //loosely checked
                    if ( (joint.Position.W < 0.8f) && (joint.TrackingState != JointTrackingState.NotTracked) )
                    {
                        //no enough tracked quality
                        isKinectTracking = false;
                        UnsetJointTracked(joint.ID);
                        continue;
                    }

                    //tracked!!!
                    SetJointTracked(joint.ID);

                    //translated to Screen XYZ
                    if (joint.Position.Z >= minDistance && joint.Position.Z <= maxDistance)
                    {

                        //record 3D position
                        JointPosition3[(int)joint.ID] = joint.Position;

                        //translate into real 2D screen
                        JointPosition2[(int)joint.ID] = getDisplayPosition(joint);

                        isKinectTracking = true;

                        //call handler
                        if (handlerClientPositionChanged[(int)joint.ID] != null)
                            OnPositionChanged(joint.ID);
                        
                    }
                }
            }

            debugMsg.WriteLine("OKOK!", DebugMsg.nui_SkeletonFrameReady);

        }        

    }
}
