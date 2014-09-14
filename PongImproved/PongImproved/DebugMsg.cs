/*
 * Kinect controlled Pong game
 * by Craig Chao, KUAS, Taiwan, R.O.C.
 * Spring 2012
 * All rights reserved, all non-permited copies are prohibited.
 * 
 */



using System;


namespace Cray.KinectTrack
{


    public class DebugMsg
    {
        //global debugging switch
        private bool debugging = true;


        //change debugging status here
        public const bool OnPositionChanged = false;
        public const bool ToggleScreenSize  = false;
        public const bool nui_Initialization = false;
        public const bool KinectControlPaddle = false;
        public const bool nui_SkeletonFrameReady = false;
        public const bool getDisplayPosition = false;
        public const bool DrawJoint = false;
        public const bool DrawJointSkeleton = false;
        public const bool RotationRadius = false;
        public const bool LightSaberDetectionHandler = false;
        public const bool MotionTracker_Initialize = false;
        public const bool MotionTracker_LoadContent = false;
        public const bool DrawLightSaber = false;
        public const bool ChangeToNextFace = false;
        public const bool GamePong_Update = false;
        public const bool G2D_Bar_Draw = false;
        public const bool PlayLaserEffect = false;

        
        public void Write(string str, Boolean debug_status)
        {
            if (debugging && debug_status)
            {
                Console.Write(str);
                //Console.Out.Flush();
            }
        }

        public void WriteLine(string str, Boolean debug_status)
        {
            if (debugging && debug_status)
            {
                Console.WriteLine(str);
                //Console.Out.Flush();
            }
        }
    }
}
