/* 字型 與 長條圖遊戲元件 鄞永傳老師 2009.04.17
 (一) 新增字型
 
 Content -> 加入 -> 新增項目 -> Sprite Font
 名稱更改為 Courier New.spritefont -> 加入

 SpriteFont Font1; // 文字
 Font1 = Content.Load<SpriteFont>("Courier New");
 
 (二) 長條圖遊戲元件
 
 GC_2D_Bar bar2D; // 宣告一個長條圖物件
 bar2D = new GC_2D_Bar(this, Font1);
 bar2D.Pos = new Vector2(100, 100);  // 擺放的位置
 bar2D.score_Max = 200;  // 設定最高的分數值
 bar2D.score = 0;  // 設定目前的分數值
 Components.Add(bar2D);  // 變成 Game1 的遊戲元件
  
 Services.AddService(typeof(SpriteBatch), spriteBatch); // Game1 要將 spriteBatch 放到服務區
  
 bar2D.score++; // 目前的分數值 加一
 bar2D.score--; // 目前的分數值 減一
 */

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
    public class G2D_Bar : Microsoft.Xna.Framework.DrawableGameComponent
    {
        Game game;
        // Texture2D Texture_Bar; // 內部的白色紋理圖

        public Vector2 Pos = Vector2.Zero; // 擺放的位置
        
        public int score_Max = 100;       // 最高的分數值 
        public int score = 0;            // 目前的的分數值 0 ~ score_Max

        public Color colorForground = Color.Blue;  // 前景顏色
        public Color colorBackground = Color.Red;// 背景顏色

        public int Width = 256; // 紋理圖 寬
        public int Height = 64; // 紋理圖 高

        private SpriteFont FontScore; // 文字
        private SpriteBatch spriteBatch;
        private KinectTracker tracker;
        private DebugMsg debugMsg;


        //private Rectangle Dest1, Dest2, Src1, Src2;


        public G2D_Bar(Game game, KinectTracker tracker, SpriteFont FontScore)
            : base(game)
        {
            // TODO: Construct any child components here
            this.game = game;
            this.tracker = tracker;
            this.FontScore = FontScore; 
        }


        protected override void LoadContent()
        {

            // TODO: Add your initialization code here
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }


        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here
            // 新增一張白色、不透明紋理圖 (Width * Height)
            /*
            Texture_Bar = new Texture2D(game.GraphicsDevice, Width, Height, true, SurfaceFormat.Color);
            Color[] color = new Color[Width * Height];
            for (int i = 0; i < Width * Height; i++)
                color[i] = new Color(255, 255, 255, 255);
            Texture_Bar.SetData(color); // 白色 不透明 紋理圖


            Dest1 = new Rectangle( // 前景區域
                    (int)Pos.X,
                    (int)Pos.Y,
                    (int)(Texture_Bar.Width * score * (1.0f / score_Max)),
                    Texture_Bar.Height);
            Dest2 = new Rectangle( // 背景區域
                                (int)(Pos.X + Texture_Bar.Width * score * (1.0f / score_Max)),
                                (int)Pos.Y,
                                (int)(Texture_Bar.Width * (score_Max - score) * (1.0f / score_Max)),
                                Texture_Bar.Height);
            Src1 = new Rectangle(0, 0, (int)(Texture_Bar.Width * score * (1.0f / score_Max)), Texture_Bar.Height);
            Src2 = new Rectangle(0, 0, (int)(Texture_Bar.Width * (score_Max - score) * (1.0f / score_Max)), Texture_Bar.Height);
            */

            debugMsg = new DebugMsg();

            base.Initialize();

            LoadContent();

        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            //SpriteBatch spriteBatch =
                //(SpriteBatch)Game.Services.GetService(typeof(SpriteBatch));
            if (tracker.GamingState == KinectTracker.GamingStates.KinectLightsaberFight)
            {

                if (spriteBatch == null) return;

                // 將分數限制在 (0, score_Max) 之間
                //score = (int)MathHelper.Clamp(score, 0, score_Max);


                // 字型設定
                string output = Convert.ToString(score); // 顯示出來的文字
                debugMsg.WriteLine("Score = (" + output+", "+score+") ", DebugMsg.G2D_Bar_Draw);

                Vector2 FontOrigin = FontScore.MeasureString(output) / 2 ; // Find the center of the string
                //Vector2 FontPos = new Vector2(Pos.X + Texture_Bar.Width * 0.5f, Pos.Y + Texture_Bar.Height + 20);
                Vector2 FontPos = new Vector2(Pos.X +120, Pos.Y  + 20);

                spriteBatch.Begin(); //SpriteSortMode.BackToFront, BlendState.AlphaBlend);

                //spriteBatch.Draw(Texture_Bar, Dest1, Src1, colorForground);
                //spriteBatch.Draw(Texture_Bar, Dest2, Src2, colorBackground);

                spriteBatch.DrawString(FontScore,     // 字型
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

            base.Update(gameTime);
        }
    }
}