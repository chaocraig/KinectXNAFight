/*
 * Kinect controlled Pong game
 * by Craig Chao, KUAS, Taiwan, R.O.C.
 * Spring 2012
 * All rights reserved, all non-permited copies are prohibited.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Cray.KinectTrack;
using Microsoft.Research.Kinect.Nui;



namespace Cray.KinectTrack
{
    public class Players : Microsoft.Xna.Framework.DrawableGameComponent
    {

        #region Private Members

        private Game game;
        private SpriteBatch spriteBatch;
        private ContentManager contentManager;
        private KinectTracker tracker;

        // bar sprite
        private RenderTarget2D Texture_Bar; // 內部的白色紋理圖

        private string[] headfiles = {"snoopy_head03", "CCLiu03","nschen03","Carol03",  "gjhwang03", "Huang03", "MingPuu03", 
                                     "tsai03"};

        public Texture2D[] spriteHead; 
        public int CurrentHeadId
        {
            set;
            get;
        }

        //debugging
        DebugMsg debugMsg = new DebugMsg();

        #endregion

        public Players(Game game, KinectTracker tracker)
            : base(game)
        {
            this.game = game;
            this.tracker = tracker;
            contentManager = new ContentManager(game.Services);

        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            spriteHead = new Texture2D[headfiles.Count()];

            int i = 0;
            foreach (string headfile in headfiles)
            {
                string fullpath = "Content\\Images\\Heads\\" + headfile;
                spriteHead[i] = contentManager.Load<Texture2D>(@fullpath);
                i++;
            }
        }


        public override void Initialize()
        {

            // 新增一張白色、不透明紋理圖 (1 * 1)
            Texture_Bar = new RenderTarget2D(game.GraphicsDevice, 1, 1);

            game.GraphicsDevice.SetRenderTarget(Texture_Bar);
            game.GraphicsDevice.Clear(Color.White);  // 白色 不透明 紋理圖
            game.GraphicsDevice.SetRenderTarget(null);

            CurrentHeadId = 0;

            base.Initialize();

            LoadContent();
        }

 
        public override void Update(GameTime gameTime)
        {

            base.Update(gameTime);
        }



        public override void Draw(GameTime gameTime)
        {
            if (tracker.GamingState >= KinectTracker.GamingStates.KinectMan )
            {
                DrawWholeBody();

                DrawHead();
            }

            //base.Draw(gameTime);
        }


        void DrawHead()
        {
            Point pos = tracker.GetJointPosition2(JointID.Head);
            Vector2 pos2 = new Vector2(pos.X - (spriteHead[CurrentHeadId].Width / 2), 
                pos.Y - spriteHead[CurrentHeadId].Height + 15);

            spriteBatch.Begin();
            spriteBatch.Draw(spriteHead[CurrentHeadId], pos2, Color.White);
            spriteBatch.End();

        }


        void DrawJointSkeleton(JointID start_joint, JointID end_joint, Color color)
        {
            //int startid=(int)start_joint, endid=(int)end_joint;

            if (!tracker.isJointTracked(start_joint) || !tracker.isJointTracked(end_joint)) return;

            debugMsg.WriteLine("DrawJointSkeleton(" + start_joint + ", " + end_joint + ", " + color + ")--("
                + tracker.GetJointPosition2(start_joint) + ", " + tracker.GetJointPosition2(end_joint) + ")"
                , DebugMsg.DrawJointSkeleton);

            DrawLine(tracker.GetJointPosition2(start_joint), tracker.GetJointPosition2(end_joint), color);

        }



        void DrawJoint(JointID joint, Color color)
        {
            Point jointPos;

            if (!tracker.isJointTracked(joint)) return;

            debugMsg.WriteLine("DrawJoint(" + joint + ", " + color + ")--(" + tracker.GetJointPosition2(joint) + ")"
                , DebugMsg.DrawJoint);

            jointPos = tracker.GetJointPosition2(joint);
            DrawBox(new Point(jointPos.X - 5, jointPos.Y - 5), new Point(jointPos.X + 5, jointPos.Y + 5), color);

        }



        void DrawLine(Point startpos, Point endpos, Color color)
        {
            float startX = startpos.X, startY = startpos.Y;
            float endX = endpos.X, endY = endpos.Y;
            //Y axis is reversed, multiply -1
            float rotation = tracker.RotationRadius(startpos, endpos);
            Vector2 barPosition = new Vector2(startX, startY);

            float Width = 10;
            //Pythagorean theorem
            float Height = (float)Math.Abs( Math.Sqrt( 
                   Math.Pow(endY - startY, 2.0) 
                +  Math.Pow(endX - startX, 2.0)  ) );

            spriteBatch.Begin();

            spriteBatch.Draw(Texture_Bar,  // 1x1的基礎白色紋理圖
                barPosition,  // 擺放的位置 (左上角座標)
                null,  // 全部圖形都要呈現
                color, // 背景顏色 red
                rotation,   // 旋轉角度
                Vector2.Zero, // 圖形的原點位置
                new Vector2(Width, Height), // 長條圖 X-寬 Y-高
                SpriteEffects.None, 0);


            spriteBatch.End();
        }


        void DrawBox(Point startpos, Point endpos, Color color)
        {
            float startX = startpos.X, startY = startpos.Y;
            float endX = endpos.X, endY = endpos.Y;
            float rotation = 0;
            Vector2 barPosition = new Vector2(startX, startY);
            float Width  = Math.Abs(endX - startX);
            float Height = Math.Abs(endY - startY);

            spriteBatch.Begin();

            spriteBatch.Draw(Texture_Bar,  // 1x1的基礎白色紋理圖
                barPosition,  // 擺放的位置 (左上角座標)
                null,  // 全部圖形都要呈現
                color, // 背景顏色 red
                rotation,   // 旋轉角度
                Vector2.Zero, // 圖形的原點位置
                new Vector2(Width, Height), // 長條圖 X-寬 Y-高
                SpriteEffects.None, 0);


            spriteBatch.End();


        }


        void DrawAllJoints()
        {
            //Head
            DrawJoint(JointID.Head, Color.Red);

            if (tracker.Joints == null) return;

            foreach (Joint joint in tracker.Joints)
            {
                JointID jid = joint.ID;

                if (jid == JointID.Count || jid == JointID.Head) continue;
                DrawJoint(jid, Color.Red);
            }
        }


        void DrawAllSkeleton()
        {
            //Head & Neck
            DrawJointSkeleton(JointID.Head, JointID.ShoulderCenter, Color.BlueViolet);

            //Right upper
            DrawJointSkeleton(JointID.HandRight, JointID.WristRight, Color.Blue);
            DrawJointSkeleton(JointID.WristRight, JointID.ElbowRight, Color.Blue);
            DrawJointSkeleton(JointID.ElbowRight, JointID.ShoulderRight, Color.Blue);
            DrawJointSkeleton(JointID.ShoulderRight, JointID.ShoulderCenter, Color.Blue);

            
            //Left upper
            DrawJointSkeleton(JointID.HandLeft, JointID.WristLeft, Color.Blue);
            DrawJointSkeleton(JointID.WristLeft, JointID.ElbowLeft, Color.Blue);
            DrawJointSkeleton(JointID.ElbowLeft, JointID.ShoulderLeft, Color.Blue);
            DrawJointSkeleton(JointID.ShoulderLeft, JointID.ShoulderCenter, Color.Blue);

            //Center
            DrawJointSkeleton(JointID.ShoulderCenter, JointID.Spine, Color.Blue);
            DrawJointSkeleton(JointID.Spine, JointID.HipCenter, Color.Blue);
            DrawJointSkeleton(JointID.HipCenter, JointID.HipRight, Color.Blue);
            DrawJointSkeleton(JointID.HipCenter, JointID.HipLeft, Color.Blue);

            //Right Leg
            DrawJointSkeleton(JointID.HipRight, JointID.KneeRight, Color.Blue);
            DrawJointSkeleton(JointID.KneeRight, JointID.AnkleRight, Color.Blue);
            DrawJointSkeleton(JointID.AnkleRight, JointID.FootRight, Color.Blue);

            //Left Leg
            DrawJointSkeleton(JointID.HipLeft, JointID.KneeLeft, Color.Blue);
            DrawJointSkeleton(JointID.KneeLeft, JointID.AnkleLeft, Color.Blue);
            DrawJointSkeleton(JointID.AnkleLeft, JointID.FootLeft, Color.Blue);

        }

        void DrawWholeBody()
        {
            if (!tracker.isKinectDeviceReady) return;

            if (tracker.GamingState >= KinectTracker.GamingStates.KinectMan)
            {

                DrawAllSkeleton();
                DrawAllJoints();
            }

        }
    }
}
