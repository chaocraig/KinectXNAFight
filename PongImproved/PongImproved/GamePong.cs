/*
 * Kinect controlled Pong game
 * by Craig Chao, KUAS, Taiwan, R.O.C.
 * Spring 2012
 * All rights reserved, all non-permited copies are prohibited.
 * 
 * Sounds: Creative Commons Sampling Plus 1.0 License.
 * http://www.freesound.org/samplesViewSingle.php?id=34201
 * http://www.freesound.org/samplesViewSingle.php?id=12658
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
using System.Windows.Forms;

using Cray.KinectTrack;
using WindowsGame.G2D;


namespace KinectPong
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class GamePong : Microsoft.Xna.Framework.Game
    {

        private GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        //float modelRotation = 0;  // 旋轉角度  全域變數
        //Matrix World, View;


        private Ball ball;
        private Paddle paddle;
        private Players player;
        private MotionTracker motion;


        //private Drawbar box2d1;

        [ThreadStaticAttribute]
        //for toggling screen size
        int screenid=0;
        TimeSpan lastToggleTime = new TimeSpan(0);

        int[,] ScreenSize = new int[4,2] {{640, 320}, {860, 480}, {1024, 768}, {1280, 800}};

        //Kinect
        KinectTracker tracker;
        DebugMsg debugMsg = new DebugMsg();


        //from WindowsGame.G2D
        G2D_ScrollingBG bg; // 宣告一個背景物件
        //G2D_Basic_PC gc; // 宣告一個 PC 物件

        G2D_Sprite_Explore myExplore;
        G2D_Sprite_Moving myFallingSprite;

        SpriteFont FontScore; // 文字
        G2D_Bar bar2D; // 宣告一個長條圖物件

        //G2D_Sprite_Bullet bullet; // 宣告一個物件
        //Texture2D myBullet; // 宣告一個2D紋理圖
        //KeyboardState oldKey;
        //List<G2D_Sprite_Bullet> bulletList = new List<G2D_Sprite_Bullet>(); // 紀錄子彈的動態陣列

        //G2D_Sprite_Explore2 myExplore2;
        //end

        public GamePong()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            tracker = new KinectTracker();
            player = new Players(this, tracker);
            ball = new Ball(this, tracker);
            paddle = new Paddle(this, tracker);
            motion = new MotionTracker(this, tracker, player); //motion detect and draw


            Components.Add(ball);
            Components.Add(paddle);
            //Components.Add(player);
            //Components.Add(motion);

            ball.CollisionSquare = paddle;

            //box2d1 = new Drawbar(this);
            //box2d2 = new Drawbar(this);

            //Components.Add(box2d1);
            //Components.Add(box2d2);


            //Player


            this.IsMouseVisible = true;
            this.Window.AllowUserResizing = true;

            // Call Window_ClientSizeChanged when screen size is changed
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += new EventHandler<EventArgs>(Window_ClientSizeChanged);


            //Kinect
            tracker.GamingState = KinectTracker.GamingStates.KinectPaddleBall;
            if (!tracker.isKinectDeviceReady)
            {
                tracker.GamingState = KinectTracker.GamingStates.PaddleBall;
                //Show Error Dialog
                Button btnOK = new Button();
                btnOK.Text = "Please check Kinect!!";
                btnOK.Width = 100; btnOK.Height = 60;
                btnOK.Left = 50;  btnOK.Top=50;

                System.Windows.Forms.Form winForm = new System.Windows.Forms.Form();
                winForm.Text = "Kinect device error!";
                winForm.Controls.Add(btnOK);
                winForm.ShowDialog();
                //Environment.Exit(-1);
            }
        }




        void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            ChangeWindowClientSize();
        }

        void ChangeWindowClientSize()
        {
            Vector2 WinSize;

            paddle.X = 100;
            // Move square back onto screen if it's off
            paddle.Y = GraphicsDevice.Viewport.Height - paddle.Height;
            if (paddle.X + paddle.Width > GraphicsDevice.Viewport.Width)
                paddle.X = GraphicsDevice.Viewport.Width - paddle.Width;

            //change Kinect's screen size
            WinSize.X = GraphicsDevice.Viewport.Width;
            WinSize.Y = GraphicsDevice.Viewport.Height;
            tracker.KinectScreenSize = WinSize;

        }

        //change screen size and change back in turn
        private void ToggleScreenSize()
        {


            int Maxid = ScreenSize.GetUpperBound(0);

            if (++screenid > Maxid) 
                screenid = 0;


            graphics.PreferredBackBufferWidth  = ScreenSize[screenid, 0];
            graphics.PreferredBackBufferHeight = ScreenSize[screenid, 1];

            debugMsg.WriteLine("Screen: "+screenid + ", " + graphics.PreferredBackBufferWidth + ", " + graphics.PreferredBackBufferHeight
                , DebugMsg.ToggleScreenSize);

            graphics.ApplyChanges();

            ChangeWindowClientSize();

        }



        private void KinectControlPaddle()
        {

            if (tracker.GamingState != KinectTracker.GamingStates.KinectPaddleBall) return;

            //if HandRight is higher than HipCenter, we controll the paddle by HandRight
            if (tracker.GetJointPosition2(JointID.HandRight).Y < tracker.GetJointPosition2(JointID.HipCenter).Y)
            {
                debugMsg.Write("Right hand! ", DebugMsg.KinectControlPaddle);

                paddle.X = tracker.GetJointPosition2(JointID.HandRight).X;
                paddle.Y = GraphicsDevice.Viewport.Height - paddle.Height - 20;
            }
            else if (tracker.GetJointPosition2(JointID.HandLeft).Y < tracker.GetJointPosition2(JointID.HipCenter).Y)
            //HandLeft is higher than HipCenter, we controll the paddle by HandLeft
            {
                debugMsg.Write("Left hand! ", DebugMsg.KinectControlPaddle);
                paddle.X = tracker.GetJointPosition2(JointID.HandLeft).X;
                paddle.Y = tracker.GetJointPosition2(JointID.HandLeft).Y;

            }

            debugMsg.WriteLine("Paddle = ( "+paddle.X + ", " + paddle.Y+ " )", DebugMsg.KinectControlPaddle);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            Vector2 WinSize;

            // Make mouse visible
            IsMouseVisible = true;

            // Set the window's title bar
            Window.Title = "Basketball Pong!";

            graphics.ApplyChanges();

            /*
            box2d1.X = 100; box2d1.Y = 100;
            box2d1.Width = 200; box2d1.Height = 10;
            box2d1.Color = Color.Red;
            box2d1.Rotation = (float) (Math.PI / 4);
             */

            //box2d2.X = 300; box2d2.Y = 300;
            //box2d2.Width = 200; box2d2.Height = 100;
            //box2d2.Color = Color.Blue;

            //Kinect specified
            if (tracker.isKinectDeviceReady)
            {
                //Environment.Exit(-1); 

                WinSize.X = GraphicsDevice.Viewport.Width;
                WinSize.Y = GraphicsDevice.Viewport.Height;
                tracker.KinectScreenSize = WinSize;

                tracker.SetJointTracked(JointID.HandRight);
                tracker.StartTracking();

                tracker.GamingState = KinectTracker.GamingStates.KinectPaddleBall;

            }

            player.Initialize();
            motion.Initialize();

            //from WindowsGame.G2D
            //background grass
            string contentPath = "Images\\";
            bg = new G2D_ScrollingBG(this, tracker, Content.Load<Texture2D>(contentPath + "grass_seamless"));
            bg.Initialize();
            bg.X_Speed = -6;
            bg.Y_Speed = 0;
            //Components.Add(bg);

            
            //rolling bombs
            myFallingSprite = new G2D_Sprite_Moving(this, tracker, motion, this.Content.Load<Texture2D>(contentPath + "Icos"), 4, 3);
            myFallingSprite.scale = 0.5f;
            myFallingSprite.Initialize();
            //special explore effect
            myExplore = new G2D_Sprite_Explore(this, tracker, this.Content.Load<Texture2D>(contentPath + "Special2"), 5, 3);
            myExplore.Initialize();
            //score bar
            FontScore = Content.Load<SpriteFont>("Segoe Keycaps");
            bar2D = new G2D_Bar(this, tracker, FontScore);
            bar2D.Pos = new Vector2(10, 10);  // 擺放的位置
            bar2D.score_Max = 200;  // 設定最高的分數值
            bar2D.score = 0;  // 設定目前的分數值
            bar2D.Initialize();
            //end
            

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            //Services.AddService(typeof(SpriteBatch), spriteBatch);

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {

            
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == Microsoft.Xna.Framework.Input.ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
                this.Exit();

            // Press F to toggle full-screen mode
            if (Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F))
            {
                //cannot change to full screen now because of Kinect
                //graphics.IsFullScreen = !graphics.IsFullScreen;

                //toggle once in two seconds, preventing keys re-entry problems
                if ((gameTime.TotalGameTime.TotalSeconds - lastToggleTime.TotalSeconds) > 2)
                {
                    //change screen size
                    ToggleScreenSize();
                    lastToggleTime = gameTime.ElapsedGameTime;
                }
            }

            //Kinect controll
            KinectControlPaddle();

            //WindwosGame.G2D
            // 檢查 掉落物 和 玩家角色 的碰撞
            if (tracker.GamingState == KinectTracker.GamingStates.KinectLightsaberFight)
            {
                Vector2 posExplosion = new Vector2(); // 發生 爆炸 的位置
                Point SaberP1 = motion.LightSaberPos[0], SaberP2 = motion.LightSaberPos[1];
                if (myFallingSprite.CheckCollision(SaberP1, SaberP2, out posExplosion))
                {
                    myExplore.AddPos(posExplosion); // 增加一個 爆炸物件
                    //gc.hit = true; // 玩家角色 抖動
                    bar2D.score++; // 目前的分數值 加一
                }
                myFallingSprite.AddMore(); // 增加 掉落的精靈圖
            }
            //end

            player.Update(gameTime);
            motion.Update(gameTime);
            bg.Update(gameTime);
            myFallingSprite.Update(gameTime);
            myExplore.Update(gameTime);

            debugMsg.WriteLine("GamePong.Update()--GamingState == "+tracker.GamingState, DebugMsg.GamePong_Update);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if (tracker.GamingState == KinectTracker.GamingStates.KinectLightsaberFight)
            {
                bg.Draw(gameTime);
                myFallingSprite.Draw(gameTime);
                myExplore.Draw(gameTime);
                bar2D.Draw(gameTime);
            }
            else
            {
                GraphicsDevice.Clear(Color.CornflowerBlue);

                if (tracker.GamingState <= KinectTracker.GamingStates.KinectMan)
                {
                    if (motion.isShowingThankyou)
                    {
                        motion.ShowThankyou();
                    }
                }
            }

            player.Draw(gameTime);
            motion.Draw(gameTime);

            base.Draw(gameTime);
        }
    }
}
