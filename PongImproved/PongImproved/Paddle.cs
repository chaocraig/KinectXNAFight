/*
 * Kinect controlled Pong game
 * by Craig Chao, KUAS, Taiwan, R.O.C.
 * Spring 2012
 * All rights reserved, all non-permited copies are prohibited.
 * 
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


namespace KinectPong
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class Paddle : Microsoft.Xna.Framework.DrawableGameComponent
    {
        #region Private Members
        private SpriteBatch spriteBatch;
        private ContentManager contentManager;
        private KinectTracker tracker;

        // square sprite
        private Texture2D squareSprite;



        // square location
        private Vector2 squarePosition;

        private const float DEFAULT_X_SPEED = 250;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the square horizontal speed.
        /// </summary>
        public float Speed { get; set; }

        /// <summary>
        /// Gets or sets the X position of the square.
        /// </summary>
        public float X
        {
            get { return squarePosition.X; }
            set { squarePosition.X = value; }
        }

        /// <summary>
        /// Gets or sets the Y position of the square.
        /// </summary>
        public float Y
        {
            get { return squarePosition.Y; }
            set { squarePosition.Y = value; }
        }

        public int Width
        {
            get { return squareSprite.Width; }
        }

        /// <summary>
        /// Gets the height of the square's sprite.
        /// </summary>
        public int Height
        {
            get { return squareSprite.Height; }
        }

        /// <summary>
        /// Gets the bounding rectangle of the square.
        /// </summary>
        public Rectangle Boundary
        {
            get
            {
                return new Rectangle((int)squarePosition.X, (int)squarePosition.Y,
                    squareSprite.Width, squareSprite.Height);
            }
        }

        #endregion

        public Paddle(Game game, KinectTracker tracker)
            : base(game)
        {
            contentManager = new ContentManager(game.Services);
            this.tracker = tracker;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            // Make sure base.Initialize() is called before this or handSprite will be null
            X = (GraphicsDevice.Viewport.Width - Width) / 2;
            Y = GraphicsDevice.Viewport.Height - Height;

            Speed = DEFAULT_X_SPEED;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            
            squareSprite = contentManager.Load<Texture2D>(@"Content\Images\hand");
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            if (tracker.GamingState != KinectTracker.GamingStates.PaddleBall
               && tracker.GamingState != KinectTracker.GamingStates.KinectPaddleBall)
                return;

            // Scale the movement based on time
            float moveDistance = Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Move square, but don't allow movement off the screen

            KeyboardState newKeyState = Keyboard.GetState();
            if (newKeyState.IsKeyDown(Keys.Right) && X + squareSprite.Width
                + moveDistance <= GraphicsDevice.Viewport.Width)
            {
                X += moveDistance;
            }
            else if (newKeyState.IsKeyDown(Keys.Left) && X - moveDistance >= 0)
            {
                X -= moveDistance;
            }
            
            

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Draw(GameTime gameTime)
        {
            if (tracker.GamingState == KinectTracker.GamingStates.PaddleBall
                || tracker.GamingState == KinectTracker.GamingStates.KinectPaddleBall)
            {

                spriteBatch.Begin();
                spriteBatch.Draw(squareSprite, squarePosition, Color.White);
                spriteBatch.End();
            }

            base.Draw(gameTime);
        }
    }
}
