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
    public class Ball : Microsoft.Xna.Framework.DrawableGameComponent
    {
        #region Private Members
        private SpriteBatch spriteBatch;
        private ContentManager contentManager;
        private KinectTracker tracker;

        // Default speed of ball
        private const float DEFAULT_X_SPEED = 150;
        private const float DEFAULT_Y_SPEED = 150;

        // Initial location of the ball
        private const float INIT_X_POS = 80;
        private const float INIT_Y_POS = 0;

        // Increase in speed each hit
        private const float INCREASE_SPEED = 50;

        // Ball image
        private Texture2D ballSprite;

        // Ball location
        Vector2 ballPosition;

        // Ball's motion
        Vector2 ballSpeed = new Vector2(DEFAULT_X_SPEED, DEFAULT_Y_SPEED);

        private SoundEffect swishSound;
        private SoundEffect crashSound;

        public Paddle CollisionSquare;

        // Used to delay between rounds 
        private float delayTimer = 0;

        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the ball's horizontal speed.
        /// </summary>
        public float SpeedX
        {
            get { return ballSpeed.X; }
            set { ballSpeed.X = value; }
        }

        /// <summary>
        /// Gets or sets the ball's vertical speed.
        /// </summary>
        public float SpeedY
        {
            get { return ballSpeed.Y; }
            set { ballSpeed.Y = value; }
        }

        /// <summary>
        /// Gets or sets the X position of the ball.
        /// </summary>
        public float X
        {
            get { return ballPosition.X; }
            set { ballPosition.X = value; }
        }

        /// <summary>
        /// Gets or sets the Y position of the ball.
        /// </summary>
        public float Y
        {
            get { return ballPosition.Y; }
            set { ballPosition.Y = value; }
        }

        /// <summary>
        /// Gets the width of the ball's sprite.
        /// </summary>
        public int Width
        {
            get { return ballSprite.Width; }
        }

        /// <summary>
        /// Gets the height of the ball's sprite.
        /// </summary>
        public int Height
        {
            get { return ballSprite.Height; }
        }

        /// <summary>
        /// Gets the bounding rectangle of the ball.
        /// </summary>
        public Rectangle Boundary
        {
            get
            {
                return new Rectangle((int)ballPosition.X, (int)ballPosition.Y,
                    ballSprite.Width, ballSprite.Height);
            }
        }
        #endregion

        public Ball(Game game, KinectTracker tracker)
            : base(game)
        {
            contentManager = new ContentManager(game.Services);
            this.tracker = tracker;
        }

        /// <summary>
        /// Set the ball at the top of the screen with default speed.
        /// </summary>
        public void Reset()
        {
            ballSpeed.X = DEFAULT_X_SPEED;
            ballSpeed.Y = DEFAULT_Y_SPEED;

            ballPosition.Y = 0;

            // Make sure ball is not positioned off the screen
            if (ballPosition.X < 0)
                ballPosition.X = 0;
            else if (ballPosition.X + ballSprite.Width > GraphicsDevice.Viewport.Width)
            {
                ballPosition.X = GraphicsDevice.Viewport.Width - ballSprite.Width;
                ballSpeed.Y *= -1;
            }
        }

        /// <summary>
        /// Increase the ball's speed in the X and Y directions.
        /// </summary>
        public void SpeedUp()
        {
            if (ballSpeed.Y < 0)
                ballSpeed.Y -= INCREASE_SPEED;
            else
                ballSpeed.Y += INCREASE_SPEED;

            if (ballSpeed.X < 0)
                ballSpeed.X -= INCREASE_SPEED;
            else
                ballSpeed.X += INCREASE_SPEED;
        }

        /// <summary>
        /// Invert the ball's horizontal direction
        /// </summary>
        public void ChangeHorzDirection()
        {
            ballSpeed.X *= -1;
        }

        /// <summary>
        /// Invert the ball's vertical direction
        /// </summary>
        public void ChangeVertDirection()
        {
            ballSpeed.Y *= -1;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {

            // Don't allow ball to move just yet
            //Enabled = false;  

            ballPosition.X = INIT_X_POS;
            ballPosition.Y = INIT_Y_POS;


            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load the texture if it exists
            ballSprite = contentManager.Load<Texture2D>(@"Content\Images\basketball");

            swishSound = contentManager.Load<SoundEffect>(@"Content\Audio\swish");
            crashSound = contentManager.Load<SoundEffect>(@"Content\Audio\crash");

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

            // Move the sprite by speed, scaled by elapsed time.
            ballPosition += ballSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Wait until a second has passed before animating ball 
            delayTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (delayTimer > 1)
                Enabled = true;

            int maxX = GraphicsDevice.Viewport.Width - Width;
            int maxY = GraphicsDevice.Viewport.Height - Height;

            // Check for bounce. Make sure to place ball back inside the screen
            // or it could remain outside the screen on the next iteration and cause
            // a back-and-forth bouncing logic error.
            if (X > maxX)
            {
                ChangeHorzDirection();
                X = maxX;
            }
            else if (X < 0)
            {
                ChangeHorzDirection();
                X = 0;
            }

            if (Y < 0)
            {
                ChangeVertDirection();
                Y = 0;
            }
            else if (Y > maxY)
            {
                // Game over - reset ball
                //crashSound.Play();
                Reset();

                // Reset timer and stop ball's Update() from executing
                delayTimer = 0;
                //Enabled = false;
            }

            // Collision?  Check rectangle intersection between ball and hand
            if (Boundary.Intersects(CollisionSquare.Boundary) && SpeedY > 0)
            {
                swishSound.Play();

                // If hitting the side of the square the ball is coming toward, 
                // switch the ball's horz direction
                float ballMiddle = (X + Width) / 2;
                float squareMiddle = (CollisionSquare.X + CollisionSquare.Width) / 2;
                if ((ballMiddle < squareMiddle && SpeedX > 0) ||
                    (ballMiddle > squareMiddle && SpeedX < 0))
                {
                    ChangeHorzDirection();
                }

                // Go back up the screen and speed up
                ChangeVertDirection();
                //ball.SpeedUp();                
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
                spriteBatch.Draw(ballSprite, ballPosition, Color.White);
                spriteBatch.End();
            }

            base.Draw(gameTime);
        }
    }
}
