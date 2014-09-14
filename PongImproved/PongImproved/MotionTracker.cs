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
using Microsoft.Research.Kinect.Nui;
using Cray.KinectTrack;


namespace Cray.KinectTrack
{
    public class MotionTracker : Microsoft.Xna.Framework.DrawableGameComponent
    {

        #region Private Members


        private Game game;
        private SpriteBatch spriteBatch;
        private ContentManager contentManager;
        private KinectTracker tracker;
        private Players player;

        // bar sprite
        private RenderTarget2D Texture_Bar; // 內部的白色紋理圖


        //debugging
        DebugMsg debugMsg = new DebugMsg();


        //Light Saber
        private SoundEffect saberon1, saberoff1, saberclash30, Slowswing1,Mediumswing1;

        private DateTime LastCallingLightSaberTimeSecs = new DateTime(0);
        private DateTime LastPlayLightSaberTimeSecs = new DateTime(0);
        private DateTime LastCallingShowManTimeSecs = new DateTime(0);
        private DateTime LastChangeFaceTimeSecs = new DateTime(0);
        private DateTime LastCallingFightTimeSecs = new DateTime(0);

        private SoundEffect[] soundEffect;
        private SpriteFont FontDisplay; // 文字


        private string[] soundFiles = { "0033", "0034", "0046", "0047", "0177", "0178" };
        private SoundEffect soundThankyou;
        private Boolean isThankyouSoundPlayed = false;

        public Boolean isShowingThankyou = false;

        #endregion


        public float[] LightSaberLen = {150, 350}; //parts which are higher & lower than lower hand
        public Point[] LightSaberPos= new Point[2]; //highest top point & lower hand position  

        public MotionTracker(Game game, KinectTracker tracker, Players player)
            : base(game)
        {
            this.game = game;
            this.player = player;
            this.tracker = tracker;
            contentManager = new ContentManager(game.Services);
      
        }


        public override void Initialize()
        {

            base.Initialize();

            LoadContent();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // 新增一張白色、不透明紋理圖 (1 * 1)
            Texture_Bar = new RenderTarget2D(game.GraphicsDevice, 1, 1);
            game.GraphicsDevice.SetRenderTarget(Texture_Bar);
            game.GraphicsDevice.Clear(Color.White);  // 白色 不透明 紋理圖
            game.GraphicsDevice.SetRenderTarget(null);


            //audio files from http://chinese-starwars.com/chineseforum/viewtopic.php?t=2931
            saberon1 = contentManager.Load<SoundEffect>(@"Content\Audio\saberon1");
            saberoff1 = contentManager.Load<SoundEffect>(@"Content\Audio\saberoff1");
            saberclash30 = contentManager.Load<SoundEffect>(@"Content\Audio\saberclash30");
            Slowswing1 = contentManager.Load<SoundEffect>(@"Content\Audio\Slowswing1");
            Mediumswing1 = contentManager.Load<SoundEffect>(@"Content\Audio\Mediumswing1");
            soundThankyou = contentManager.Load<SoundEffect>(@"Content\Audio\tarzan2");

            //change face sound effects
            soundEffect = new SoundEffect[soundFiles.Length];
            int i = 0;
            foreach (string sound in soundFiles)
            {
                soundEffect[i] = contentManager.Load<SoundEffect>("Content\\Audio\\ChangeFace\\" + sound);
                i++;
            }
            FontDisplay = contentManager.Load<SpriteFont>("Content\\Jing Jing");

            tracker.SetKinectPositionChangedHandler(JointID.HandRight, RightHandMoveHandler);

            debugMsg.WriteLine("MotionTracker.Initialize()", DebugMsg.MotionTracker_LoadContent);
        }


        public override void Update(GameTime gameTime)
        {
            //ElapsedGameTimeTotalSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            //ElapsedGameTimeSecs = (int)gameTime.ElapsedGameTime.TotalSeconds;

            base.Update(gameTime);
        }



        public override void Draw(GameTime gameTime)
        {
            if (tracker.GamingState >= KinectTracker.GamingStates.KinectLightSaber)
            {
                    DrawLightSaber();
            }

            //base.Draw(gameTime);
        }

        Rectangle RectIntersects(JointID jid, int LenDetion)
        {
            Point pos;

            pos = tracker.GetJointPosition2(jid);
            Rectangle RectJoint = new Rectangle(pos.X - LenDetion, pos.Y - LenDetion, 2 * LenDetion, 2 * LenDetion);

            return (RectJoint);
        }


        /// <summary>
        /// 判斷兩條線段是否相交。
        /// </summary>
        /// <param name="line1">線段1</param>
        /// <param name="line2">線段2</param>
        /// <returns>相交返回真，否則返回假。</returns>
        public bool CheckTwoLineCrose(Point Line1P1, Point Line1P2, Point Line2P1, Point Line2P2)
        {
            return CheckCrossLine(Line1P1, Line1P2, Line2P1, Line2P2)
                && CheckCrossLine(Line2P1, Line2P2, Line1P1, Line1P2);
        }
        /// <summary>
        /// 計算兩個向量的叉乘。
        /// </summary>
        /// <param name="pt1"></param>
        /// <param name="pt2"></param>
        /// <returns></returns>
        private float CrossMul(Point pt1, Point pt2)
        {
            return pt1.X * pt2.Y - pt1.Y * pt2.X;
        }

        /// <summary>
        /// 判斷直線2的兩點是否在直線1的兩邊。
        private Boolean CheckCrossLine(Point Line1P1, Point Line1P2, Point Line2P1, Point Line2P2)
        {
            Point v1 = new Point();
            Point v2 = new Point();
            Point v3 = new Point();

            v1.X = Line2P1.X - Line1P2.X;
            v1.Y = Line2P1.Y - Line1P2.Y;

            v2.X = Line2P2.X - Line1P2.X;
            v2.Y = Line2P2.Y - Line1P2.Y;

            v3.X = Line1P1.X - Line1P2.X;
            v3.Y = Line1P1.Y - Line1P2.Y;

            return (CrossMul(v1, v3) * CrossMul(v2, v3) <= 0);
        }


        //detect player's gesture of light saber
        //left hand (Y)is lower than hip and (X)away from hip, 
        //   and the right hand (Y) is at neck and (X) near head or shoulder center
        Boolean isCallingLightSaber()
        {
            Point HandLeft = tracker.GetJointPosition2(JointID.HandLeft);
            Point HandRight = tracker.GetJointPosition2(JointID.HandRight);
            Point HipLeft = tracker.GetJointPosition2(JointID.HipLeft);
            Point HipRight = tracker.GetJointPosition2(JointID.HipRight);
            Point ShoulderCenter = tracker.GetJointPosition2(JointID.ShoulderCenter);
            Point Head = tracker.GetJointPosition2(JointID.Head);

            if ( (HandRight.Y > Head.Y) && (HandRight.Y < ShoulderCenter.Y)
                && (HandLeft.Y > HipLeft.Y) && ((HipLeft.X - HandLeft.X) > (HipRight.X - HipLeft.X)) )
            {
                return true;
            }
            return false;

        }

        //detect player's gesture of light saber
        //both hands hold on each other and put near head
        Boolean isCallingShowMan()
        {
            const int LenDetion = 20;   //1/2 of detection length
            Rectangle RectHandLeft, RectHandRight, RectHead;

            RectHandLeft = RectIntersects(JointID.HandLeft, LenDetion);
            RectHandRight = RectIntersects(JointID.HandRight, LenDetion);
            RectHead = RectIntersects(JointID.Head, LenDetion);

            if (RectHandLeft.Intersects(RectHandRight) && RectHandRight.Intersects(RectHead))
                return true;
            else
                return false;
        }


        //detect to change player's face
        //right hand (Y)is lower than hip and (X)away from hip, 
        //   and left hand (Y) is at neck and (X) near head or shoulder center
        Boolean isCallingFaceChange()
        {
            Point HandLeft = tracker.GetJointPosition2(JointID.HandLeft);
            Point HandRight = tracker.GetJointPosition2(JointID.HandRight);
            Point HipLeft = tracker.GetJointPosition2(JointID.HipLeft);
            Point HipRight = tracker.GetJointPosition2(JointID.HipRight);
            Point ShoulderCenter = tracker.GetJointPosition2(JointID.ShoulderCenter);
            Point Head = tracker.GetJointPosition2(JointID.Head);

            if ((HandLeft.Y > Head.Y) && (HandLeft.Y < ShoulderCenter.Y) 
                && (HandRight.Y > HipRight.Y) && ((HandRight.X - HipRight.X) > (HipRight.X-HipLeft.X)) )
            {
                return true;
            } 
            return false;
        }


        //detect to show/hide fighting field and bomps
        //both hand (Y)are higher than head and (X)far away each other
        Boolean isCallingFight()
        {
            Point HandLeft = tracker.GetJointPosition2(JointID.HandLeft);
            Point HandRight = tracker.GetJointPosition2(JointID.HandRight);
            //Point HipLeft = tracker.GetJointPosition2(JointID.HipLeft);
            //Point HipRight = tracker.GetJointPosition2(JointID.HipRight);
            Point ShoulderLeft = tracker.GetJointPosition2(JointID.ShoulderLeft);
            Point ShoulderRight = tracker.GetJointPosition2(JointID.ShoulderRight);
            Point Head = tracker.GetJointPosition2(JointID.Head);

            if (   (HandLeft.Y < Head.Y)  && (HandRight.Y < Head.Y) 
                && ((HandRight.X - HandLeft.X) > (ShoulderRight.X - ShoulderLeft.X) * 2) )
            {
                    return true;
            }
            return false;
        }


        Boolean isCallingThankyou()
        {
            Point Head = tracker.GetJointPosition2(JointID.Head);
            Point ShoulderCenter = tracker.GetJointPosition2(JointID.ShoulderCenter);

            if (Head.Y > ShoulderCenter.Y)
            {
                isShowingThankyou = true;
                return true;
            }
            return false;
        }

        //switch the state, then the LightSaber will draw
        void SwitchLightSaberStatus()
        {

            if (tracker.GamingState == KinectTracker.GamingStates.KinectMan)
            {
                tracker.GamingState = KinectTracker.GamingStates.KinectLightSaber;
                saberon1.Play();
            }
            else if (tracker.GamingState == KinectTracker.GamingStates.KinectLightSaber)
            {
                tracker.GamingState = KinectTracker.GamingStates.KinectMan;
                saberoff1.Play();
            }
        }

        //switch the state, then the Player() will draw
        void SwitchShowManStatus()
        {
            if (tracker.GamingState == KinectTracker.GamingStates.KinectMan)
            {
                tracker.GamingState = KinectTracker.GamingStates.KinectPaddleBall;
                saberon1.Play();
            }
            else if (tracker.GamingState == KinectTracker.GamingStates.KinectPaddleBall)
            {
                tracker.GamingState = KinectTracker.GamingStates.KinectMan;
                saberoff1.Play();
            }

        }

        //switch the state, then the Player() will draw
        void SwitchShowFightStatus()
        {
            if (tracker.GamingState == KinectTracker.GamingStates.KinectLightsaberFight)
            {
                tracker.GamingState = KinectTracker.GamingStates.KinectLightSaber;
                saberon1.Play();
            }
            else if (tracker.GamingState == KinectTracker.GamingStates.KinectLightSaber)
            {
                tracker.GamingState = KinectTracker.GamingStates.KinectLightsaberFight;
                saberoff1.Play();
            }

        }

        //detect player by his/her right hand
        private void RightHandMoveHandler(object sender, JointID id)
        {
            //sender == KinectTracker

            switch (tracker.GamingState) {
                case KinectTracker.GamingStates.KinectPaddleBall:
                    KinectManDetection();
                    ThankyouDetection();
                    break;
                case KinectTracker.GamingStates.KinectMan:
                    LightSaberDetection();
                    KinectManDetection();
                    ChangeFaceDetection();
                    ThankyouDetection();
                    break;
                case KinectTracker.GamingStates.KinectLightSaber:
                    LightSaberDetection();
                    FightDetection();
                    break;
                case KinectTracker.GamingStates.KinectLightsaberFight:
                    FightDetection();
                    break;
                default:
                    break;
            }
        }


        void PlayChangeFaceEffect()
        {
            Random rnd = new Random();

            int id = rnd.Next(0, soundEffect.Length-1);
                soundEffect[id].Play();
        }


        public void ShowThankyou()
        {

                if (tracker.GamingState > KinectTracker.GamingStates.KinectMan) return;

                if (!isShowingThankyou) return;

                if (!isThankyouSoundPlayed)
                {
                    soundThankyou.Play();

                    isThankyouSoundPlayed = true;
                }

                string output = "Thank You !";
                Vector2 FontOrigin = FontDisplay.MeasureString(output) / 2 ; // the center of this string
                //Vector2 FontPos = new Vector2(Pos.X + Texture_Bar.Width * 0.5f, Pos.Y + Texture_Bar.Height + 20);
                Vector2 FontPos = tracker.KinectScreenSize / 2;

                spriteBatch.Begin(); //SpriteSortMode.BackToFront, BlendState.AlphaBlend);

                //spriteBatch.Draw(Texture_Bar, Dest1, Src1, colorForground);
                //spriteBatch.Draw(Texture_Bar, Dest2, Src2, colorBackground);

                spriteBatch.DrawString(FontDisplay,     // 字型
                                  output,         // 字串
                                  FontPos,        // 位置
                                  Color.White, // 字的顏色
                                  0,           // 旋轉角度
                                  FontOrigin,  // 字串中心點
                                  1.0f,        // 縮放倍數
                                  SpriteEffects.None,
                                  1);       // 圖層深度 0.0 ~ 1.0 (後)

                spriteBatch.End();
            

        }

        void ChangeToNextFace()
        {


            PlayChangeFaceEffect();

            player.CurrentHeadId++;

            if (player.CurrentHeadId >= player.spriteHead.Count())
            {
                player.CurrentHeadId = 0;  //reset to first one
            }

            debugMsg.WriteLine("ChangeToNextFace()--player.CurrentHeadId = " + player.CurrentHeadId, DebugMsg.ChangeToNextFace);
        }

        //detect the gestue to show/hide player's skeleton
        private void KinectManDetection()
        {

            int secs = ElapsedSeconds(LastCallingShowManTimeSecs);

            if (secs > 3) //don't allow re-entry twice in 3 seconds
            {
                if (isCallingShowMan())
                {
                    SwitchShowManStatus();
                    LastCallingShowManTimeSecs = DateTime.Now;
                }
            }
        }

        //detect the gestue to show/hide fighting
        private void FightDetection()
        {

            int secs = ElapsedSeconds(LastCallingFightTimeSecs);

            if (secs > 3) //don't allow re-entry twice in 3 seconds
            {
                if (isCallingFight())
                {
                    SwitchShowFightStatus();
                    LastCallingFightTimeSecs = DateTime.Now;
                }
            }
        }


        void ChangeFaceDetection()
        {
            int secs = ElapsedSeconds(LastChangeFaceTimeSecs);

            if (secs >= 2) //don't allow re-entry twice in 3 seconds
            {
                if (isCallingFaceChange())
                {
                    ChangeToNextFace();
                    LastChangeFaceTimeSecs = DateTime.Now;
                }
            }
        }

        void ThankyouDetection()
        {
            if (tracker.GamingState > KinectTrack.KinectTracker.GamingStates.KinectMan) return;

            isCallingThankyou();

            //played in main().draw();
        }

        private int ElapsedSeconds(DateTime LastTime)
        {
            DateTime ElapsedGameTimeSecs = DateTime.Now;

            int secs = ElapsedGameTimeSecs.Subtract(LastTime).Seconds;
            return (secs);
        }


        //detect player show/hide light saber
        private void LightSaberDetection()
        {

            if (tracker.Joints == null) return;

            int secs = ElapsedSeconds(LastCallingLightSaberTimeSecs);

            if (secs > 3) //don't allow re-entry twice in 3 seconds
            {
                if (isCallingLightSaber())
                {
                    SwitchLightSaberStatus();
                    LastCallingLightSaberTimeSecs = DateTime.Now;
                }
            }
            debugMsg.WriteLine("LightSaberDetection()-tracker.GamingState = " + tracker.GamingState, DebugMsg.LightSaberDetectionHandler);
        }



        void DrawLightSaber()
        {

            Point high, low, startpos;

            if (tracker.Joints == null) return;
            if (tracker.GamingState < KinectTracker.GamingStates.KinectLightSaber) return;

            //play sound
            int secs = ElapsedSeconds(LastPlayLightSaberTimeSecs);
            if ( secs > 2)  //don't re-entry in 2 seconds
            {
                secs = LastPlayLightSaberTimeSecs.Second - ((LastPlayLightSaberTimeSecs.Second / 10) * 10);
                debugMsg.WriteLine("Play secs = " + secs, DebugMsg.DrawLightSaber);

                if (secs >= 4)  // 1/2 chance
                    Mediumswing1.Play();
                else
                    Slowswing1.Play();
                LastPlayLightSaberTimeSecs = DateTime.Now;
            }

            //decide which hand is up
            if (tracker.GetJointPosition2(JointID.HandLeft).Y < tracker.GetJointPosition2(JointID.HandRight).Y)
            {
                high = tracker.GetJointPosition2(JointID.HandLeft);
                low = tracker.GetJointPosition2(JointID.HandRight);
            }
            else
            {
                high = tracker.GetJointPosition2(JointID.HandLeft);
                low = tracker.GetJointPosition2(JointID.HandRight);
            }

            float len1 = LightSaberLen[0], len2 = LightSaberLen[1];
            float lenHighLow;

            lenHighLow = (float)Math.Sqrt(Math.Pow(high.Y - low.Y, 2) + Math.Pow(high.X - low.X, 2)); //length between higher point and lower point
            startpos.X = low.X + (int)(len2 / lenHighLow) * (high.X - low.X);
            startpos.Y = low.Y + (int)(len2 / lenHighLow) * (high.Y - low.Y);

            //update positions of LightSaber
            LightSaberPos[0] = startpos;
            LightSaberPos[1] = low;

            //Y axis is reversed, multiply -1
            float rotation = tracker.RotationRadius(startpos, low);
            Vector2 barPosition = new Vector2(startpos.X, startpos.Y);

            float Width = 10;
            //Pythagorean theorem
            float Height = len1 + len2;

            spriteBatch.Begin();

            spriteBatch.Draw(Texture_Bar,  // 1x1的基礎白色紋理圖
                barPosition,  // 擺放的位置 (左上角座標)
                null,  // 全部圖形都要呈現
                Color.Yellow, // 背景顏色 
                rotation,   // 旋轉角度
                Vector2.Zero, // 圖形的原點位置
                new Vector2(Width, Height), // 長條圖 X-寬 Y-高
                SpriteEffects.None, 0);


            spriteBatch.End();
        }


    }
}
