/* 滾動的背景圖遊戲元件  鄞永傳老師  98.04.14
   GC_2D_ScrollingBG bg;
  
   bg = new GC_2D_ScrollingBG(this, Content.Load<Texture2D>("alien_riverbed"));
   Components.Add(bg);
   bg.Y_Speed = 2;
 
   // 主程式 要加入 spriteBatch 服務
   Services.AddService(typeof(SpriteBatch), spriteBatch);
 * */
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;
using Cray.KinectTrack;

namespace WindowsGame.G2D
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class G2D_ScrollingBG : Microsoft.Xna.Framework.DrawableGameComponent
    {
        Texture2D my;
        int Offset_X = 0;  // X 軸 偏移値
        int Offset_Y = 0;  // Y 軸 偏移値
        public int X_Speed = 0; // -1 向左  +1 向右
        public int Y_Speed = 1; // -1 向上  +1 向下
        Game game;
        double ElapsedTime = 0;

        private SpriteBatch spriteBatch;
        private KinectTracker tracker;

        public G2D_ScrollingBG(Game game, KinectTracker tracker, Texture2D my)
            : base(game)
        {
            // TODO: Construct any child components here
            this.game = game;
            this.my = my;
            this.tracker = tracker;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            LoadContent();
        }

        protected override void LoadContent()
        {

            // TODO: Add your initialization code here
            spriteBatch = new SpriteBatch(GraphicsDevice);

        }


        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            if (tracker.GamingState == KinectTracker.GamingStates.KinectLightsaberFight)
            {

                // TODO: Add your update code here
                ElapsedTime += gameTime.ElapsedGameTime.TotalMilliseconds; // 兩個 Frame 間隔的千分秒
                if (ElapsedTime < 100) return;
                ElapsedTime = 0;

                Offset_X += X_Speed;  // -1 向左  +1 向右
                Offset_Y += Y_Speed;  // -1 向上  +1 向下
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (tracker.GamingState == KinectTracker.GamingStates.KinectLightsaberFight)
            {
                //SpriteBatch spriteBatch =
                    //(SpriteBatch)Game.Services.GetService(typeof(SpriteBatch));

                if (spriteBatch == null) return;

                int W = game.Window.ClientBounds.Width;
                int H = game.Window.ClientBounds.Height;

                spriteBatch.Begin();
                for (int i = -1; i <= W / my.Width + 1; i++)        // i 是 X 方向 要貼幾次
                {
                    for (int j = -1; j <= H / my.Height + 1; j++)  // j 是 Y 方向 要貼幾次
                    {
                        Vector2 position = new Vector2(
                                           i * my.Width + (Offset_X % my.Width),
                                           j * my.Height + (Offset_Y % my.Height)); // 算出要 貼上的位置

                        spriteBatch.Draw(my, position, Color.White);
                    }
                }
                spriteBatch.End();
            }
            base.Draw(gameTime);
        }

    }
}