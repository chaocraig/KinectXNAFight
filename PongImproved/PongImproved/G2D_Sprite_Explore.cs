/* 多個爆炸點遊戲元件： 鄞永傳老師 2009.04.14
 
  GC_2D_Sprite_Explore myExplore; // 宣告一個物件
  
  // Special2 有 5 X 3 張小圖
  myExplore = new GC_2D_Sprite_Explore(this, this.Content.Load<Texture2D>("Special2"), 5, 3);
  Components.Add(myExplore);
  
  myExplore.AddPos( 代表爆炸位置的 Vector2 ); // mySpriteList[i].pos);
 
  Services.AddService(typeof(SpriteBatch), spriteBatch); // Game1 要將 spriteBatch 放到服務區
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
    public class G2D_Sprite_Explore : Microsoft.Xna.Framework.DrawableGameComponent
    {
        Texture2D my;
        public double scale = 1; // 圖形縮放倍數
        public byte alpha = 255; // 不透明度
        List<Point> ICON_List = new List<Point>(); // 紀錄小圖的位置
        Point ICON_Size = new Point();   // 小圖的寬高
        double ElapsedTime = 0;
        public double FrameSpeed = 10; // 小圖間隔時間 (預設值為 10 個千分秒)

        List<Vector2> myPosList = new List<Vector2>();     // 紀錄 爆炸的位置
        List<short> myCurrent_NoList = new List<short>();  // 紀錄 爆炸的圖形次序編號

        private KinectTracker tracker;
        private SpriteBatch spriteBatch;
        private SoundEffect[] soundEffect;
        private ContentManager contentManager;
        private DebugMsg debugMsg;

        private string[] soundFiles = { "Laser", "Laser0115", "Laser0116", "Laser0117", "Laser0126" };


        // 加入一個座標
        public void AddPos(Vector2 pos)
        {
            myPosList.Add(pos);
            myCurrent_NoList.Add(0);
        }

        public int Count  // 屬性 幾張小圖
        {
            get
            {
                return ICON_List.Count;
            }
        }

        public G2D_Sprite_Explore(Game game, KinectTracker tracker, Texture2D my, short col, short row)
            : base(game)
        {
            // TODO: Construct any child components here
            this.my = my;
            this.tracker = tracker;
            contentManager = new ContentManager(game.Services);

            Point point = new Point();

            ICON_Size.X = my.Width / col;  // 每張小圖的寬
            ICON_Size.Y = my.Height / row;

            for (int i = 0; i < row; i++)  // i 往下走
                for (int j = 0; j < col; j++) // j 往右走
                {
                    point.X = j * ICON_Size.X;  // j 乘成上 小圖的寬
                    point.Y = i * ICON_Size.Y;  // i 乘成上 小圖的高
                    ICON_List.Add(point);
                }
            //Current_No = 0;  // 目前是第幾張小圖
        }



        protected override void LoadContent()
        {

            // TODO: Add your initialization code here
            spriteBatch = new SpriteBatch(GraphicsDevice);


            soundEffect = new SoundEffect[soundFiles.Length];
            int i = 0;
            foreach (string sound in soundFiles)
            {
                soundEffect[i] = contentManager.Load<SoundEffect>("Content\\Audio\\"+sound);
                i++;
            }
        }



        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here

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

            if (tracker.GamingState == KinectTracker.GamingStates.KinectLightsaberFight)
            {

                // TODO: Add your update code here
                ElapsedTime += gameTime.ElapsedGameTime.TotalMilliseconds; // 兩個 Frame 間隔的千分秒
                if (ElapsedTime > FrameSpeed)
                {
                    ElapsedTime = 0;

                    for (int i = myCurrent_NoList.Count - 1; i >= 0; i--)
                    {
                        myCurrent_NoList[i]++;
                        if (myCurrent_NoList[i] >= ICON_List.Count)
                        {
                            myCurrent_NoList.RemoveAt(i);
                            myPosList.RemoveAt(i);
                        }
                    }
                }
            }
            //base.Update(gameTime);
        }


        void PlayLaserEffect()
        {
            Random rnd = new Random();

            int id = rnd.Next(1,10);
            debugMsg.WriteLine("Explore sound = " + id, DebugMsg.PlayLaserEffect);

            if (id < soundEffect.Length)
            {
                soundEffect[id].Play();
            }
            else
            {
                soundEffect[0].Play();  //mostly played
            }
        }

        public override void Draw(GameTime gameTime)
        {
            if (tracker.GamingState == KinectTracker.GamingStates.KinectLightsaberFight)
            {

                //SpriteBatch spriteBatch =
                    //(SpriteBatch)Game.Services.GetService(typeof(SpriteBatch));

                if (spriteBatch == null) return;

                if (myPosList.Count >0) PlayLaserEffect();

                spriteBatch.Begin();

                for (int i = 0; i < myPosList.Count; i++)
                {
                    Rectangle dest;
                    dest.Width = (int)(ICON_Size.X * scale);
                    dest.Height = (int)(ICON_Size.Y * scale);
                    dest.X = (int)myPosList[i].X - dest.Width / 2;
                    dest.Y = (int)myPosList[i].Y - dest.Height / 2;

                    Rectangle src;
                    src.Width = ICON_Size.X;
                    src.Height = ICON_Size.Y;
                    src.X = ICON_List[myCurrent_NoList[i]].X;
                    src.Y = ICON_List[myCurrent_NoList[i]].Y;

                    spriteBatch.Draw(my,   // 2D Texture
                                     dest,   // 目的區的 矩形區塊
                                     src,   // 來源區的 矩形區塊，null就是 全圖
                                     new Color(255, 255, 255, alpha), // 顏色 濾鏡 + 不透明度
                                     0,  // 旋轉徑度
                                     Vector2.Zero, // 2D 紋理圖的旋轉中心點
                                     SpriteEffects.None,  // 旋轉效果
                                     1.0f); // 圖層深度 0.0 ~ 1.0 (後)
                }

                spriteBatch.End();
            }
            //base.Draw(gameTime);
        }

    }
}