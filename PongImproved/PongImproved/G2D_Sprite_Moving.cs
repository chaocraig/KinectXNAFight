/* 多個往右移動上下起伏的精靈圖遊戲元件：  鄞永傳老師 2009.04.21
 
  GC_2D_Sprite_Moving myFallingSprite; // 宣告一個物件
  
  // plus 有  6 x 4 張小圖
  myFallingSprite = new GC_2D_Sprite_Moving(this, this.Content.Load<Texture2D>("plus"), 6, 4);
  Components.Add(myFallingSprite);
  
  // 在主程式 protected override void Update(GameTime gameTime) 中
  // posExplosion 是 爆炸點 位置
  // gc.pos 是 玩家角色     gc.hit 是 玩家是否受到撞擊
  // myExplore 是 多個爆炸點遊戲元件
  // CheckCollision() 是 檢查 全部精靈圖 是否有 和 gc.pos 碰撞
  Vector2 posExplosion = new Vector2();
  if (myFallingSprite.CheckCollision(gc.pos, out posExplosion, 100)) // 100 是 碰撞的邊界距離
    {
      myExplore.AddPos(posExplosion);
      gc.hit = true;
    }
  myFallingSprite.AddMore();  // 增加 掉落的精靈圖
 
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
    public class G2D_Sprite_Moving : Microsoft.Xna.Framework.DrawableGameComponent
    {
        Texture2D my;
        public double scale = 1; // 圖形縮放倍數
        public byte alpha = 255; // 不透明度
        List<Point> ICON_List = new List<Point>(); // 紀錄小圖的位置
        Point ICON_Size = new Point();   // 小圖的寬高
        double ElapsedTime = 0;
        public double FrameSpeed = 100; // 小圖間隔時間 (預設值為 10 個千分秒)

        List<Vector2> myPosList = new List<Vector2>();
        List<short> myCurrent_NoList = new List<short>();
        List<double> myThetaList = new List<double>();
        Game game;
        Random rd = new Random();
        //int var = 10; // 橫向亂數

        private KinectTracker tracker;
        private MotionTracker motion;
        private SpriteBatch spriteBatch;

        public bool CheckCollision(Point LightSaberStartP,Point LightSaberEndP, out Vector2 expPos)
        {
            for (int i = myPosList.Count - 1; i >= 0; i--)
            {
                Point iconStartP, iconEndP;

                iconStartP.X = (int)myPosList[i].X;  iconStartP.Y = (int)myPosList[i].Y;
                iconEndP.X = (int)myPosList[i].X+ICON_Size.X;  iconEndP.Y = (int)myPosList[i].Y+ICON_Size.Y;


                Boolean isCollision = motion.CheckTwoLineCrose(LightSaberStartP, LightSaberEndP, iconStartP, iconEndP);

                if (isCollision)
                {
                    expPos = myPosList[i];
                    myPosList.RemoveAt(i);
                    myCurrent_NoList.RemoveAt(i);

                    return true;
                }
            }
            expPos = new Vector2();
            return false;
        }

/*
        public bool CheckCollision(Vector2 pcPos, out Vector2 expPos, int collisionLeng)
        {
            for (int i = myPosList.Count - 1; i >= 0; i--)
            {
                double dist = Math.Sqrt((myPosList[i].X - pcPos.X) * (myPosList[i].X - pcPos.X) +
                                        (myPosList[i].Y - pcPos.Y) * (myPosList[i].Y - pcPos.Y));

                if (dist < collisionLeng)
                {
                    expPos = myPosList[i];
                    myPosList.RemoveAt(i);
                    myCurrent_NoList.RemoveAt(i);

                    return true;
                }
            }
            expPos = new Vector2();
            return false;
        }
*/

        // 加入一個座標
        public void AddMore()
        {
            if (rd.NextDouble() < 0.1)
            {
                //myPosList.Add(new Vector2(0, rd.Next(game.Window.ClientBounds.Height - 50) + 50));
                myPosList.Add(new Vector2(game.Window.ClientBounds.Width, rd.Next(game.Window.ClientBounds.Height - 50) + 50));
                myCurrent_NoList.Add(0);
                myThetaList.Add(0);
            }
        }

        public int Count  // 屬性 幾張小圖
        {
            get
            {
                return ICON_List.Count;
            }
        }

        public G2D_Sprite_Moving(Game game, KinectTracker tracker, MotionTracker motion, Texture2D my, short col, short row)
            : base(game)
        {
            // TODO: Construct any child components here
            this.game = game;
            this.my = my;
            this.tracker = tracker;
            this.motion = motion;

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
                            myCurrent_NoList[i] = 0;

                        // 設法改變座標
                        myThetaList[i] += 0.1;
                        myPosList[i] = new Vector2(myPosList[i].X - 10, (float)(myPosList[i].Y + 6 * Math.Sin(myThetaList[i])));

                        //if (myPosList[i].X > game.Window.ClientBounds.Width)
                        if (myPosList[i].X < 0)
                        {
                            myPosList.RemoveAt(i);
                            myCurrent_NoList.RemoveAt(i);
                        }
                    }
                    //Current_No++;
                    //if (Current_No >= ICON_List.Count) Current_No = 0;
                }
            }

            //base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (tracker.GamingState == KinectTracker.GamingStates.KinectLightsaberFight)
            {
                //SpriteBatch spriteBatch =
                    //(SpriteBatch)Game.Services.GetService(typeof(SpriteBatch));

                if (spriteBatch == null) return;

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