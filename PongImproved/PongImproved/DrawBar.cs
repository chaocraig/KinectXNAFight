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


namespace KinectPong
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class Drawbar : Microsoft.Xna.Framework.DrawableGameComponent
    {
        #region Private Members

        private Game game;
        private SpriteBatch spriteBatch;
        private ContentManager contentManager;

        // bar sprite
        //private Texture2D barSprite;
        private RenderTarget2D Texture_Bar; // 內部的白色紋理圖
        //private Vector2 Pos = new Vector2(100,100); // 擺放的位置


        // bar location
        private Vector2 barPosition = new Vector2(100, 100); // 擺放的位置
        //private Color ForColor = Color.Blue;  // 前景顏色
        private Color BackColor = Color.Red;  // 背景顏色
        private int barWidth = 1; // 長條圖 寬
        private int barHeight = 1; // 長條圖 高


        #endregion
        
        #region Properties


        /// <summary>
        /// Gets or sets the X position of the bar.
        /// </summary>
        public float X
        {
            get { return barPosition.X; }
            set { barPosition.X = value; }
        }

        /// <summary>
        /// Get/set or sets the Y position of the bar.
        /// </summary>
        public float Y
        {
            get { return barPosition.Y; }
            set { barPosition.Y = value; }
        }

        public int Width
        {
            get { return barWidth; }
            set { barWidth = value; }
        }

        /// <summary>
        /// Get/set the height of the bar's sprite.
        /// </summary>
        public int Height
        {
            get { return barHeight; }
            set { barHeight = value; }
        }

        /// <summary>
        /// Get/set the color of the bar.
        /// </summary>
        public Color Color
        {
            get { return BackColor; }
            set { BackColor = value; }
        }

        /// <summary>
        /// Get/set the rotation of the bar.
        /// </summary>
        public float Rotation
        {   get;  set; 
        }

        /// <summary>
        /// Gets the bounding rectangle of the bar.
        /// </summary>
        public Rectangle Boundary
        {
            get
            {
                return new Rectangle((int)barPosition.X, (int)barPosition.Y,
                    Width, Height);
            }
        }

        #endregion

        public Drawbar(Game game)
            : base(game)
        {
            this.game = game;
            contentManager = new ContentManager(game.Services);
        }


        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {


            // Make sure base.Initialize() is called before this or handSprite will be null
            //X = (GraphicsDevice.Viewport.Width - Width) / 2;
            //Y = GraphicsDevice.Viewport.Height - Height;

            // 新增一張白色、不透明紋理圖 (1 * 1)
            Texture_Bar = new RenderTarget2D(game.GraphicsDevice, 1, 1);
            game.GraphicsDevice.SetRenderTarget(Texture_Bar);
            game.GraphicsDevice.Clear(Color.White);  // 白色 不透明 紋理圖
            game.GraphicsDevice.SetRenderTarget(null);

            //Width = Texture_Bar.Width;
            //Height = Texture_Bar.Height;

            base.Initialize();

        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            
            spriteBatch = new SpriteBatch(GraphicsDevice);
             
            /*
            barSprite = contentManager.Load<Texture2D>(@"Content\Images\hand");
             */
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Draw(GameTime gameTime)
        {           
            spriteBatch.Begin();

            spriteBatch.Draw(Texture_Bar,  // 1x1的基礎白色紋理圖
               barPosition,  // 擺放的位置 (左上角座標)
               null,  // 全部圖形都要呈現
               Color, // 背景顏色 red
               Rotation,   // 旋轉角度
               Vector2.Zero, // 圖形的中心位置
               new Vector2(Width, Height), // 長條圖 寬 高
               SpriteEffects.None, 0);

            
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
