/* 大爆炸遊戲元件： 鄞永傳老師 2009.04.22
 
  GC_2D_Sprite_Explore2 myExplore2; // 宣告一個物件
  
  // 有 5 X 2 張小圖
  myExplore2 = new GC_2D_Sprite_Explore2(this, this.Content.Load<Texture2D>("Earth1"), 5, 2);
  Components.Add(myExplore2);  // 變成 Game1 的遊戲元件
  
  // 主程式 Update(GameTime gameTime)
  // 啟動 大爆炸 
  if (newKey.IsKeyDown(Keys.Enter) && oldKey.IsKeyUp(Keys.Enter))
     myExplore2.AddPos(gc.pos);
 
  // 主程式 Update(GameTime gameTime)
  // 檢查 掉落物 和 大爆炸 的碰撞
     if (myExplore2.alive)
        {
           if (myFallingSprite.CheckCollision(gc.pos, out posExplosion, 500))
             {
               myExplore.AddPos(posExplosion); // 增加一個 爆炸物件
               gc.hit = true; // 玩家角色 抖動
               bar2D.score++; // 目前的分數值 加一
             }
        }
 
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
    public class G2D_Sprite_Explore2 : Microsoft.Xna.Framework.DrawableGameComponent
    {
        Texture2D my;
        public double scale = 5; // 圖形縮放倍數
        public byte alpha = 255; // 不透明度
        List<Point> ICON_List = new List<Point>(); // 紀錄小圖的位置
        Point ICON_Size = new Point();   // 小圖的寬高
        double ElapsedTime = 0;
        public double FrameSpeed = 100; // 小圖間隔時間 (預設值為 10 個千分秒)

        Vector2 myPos = new Vector2();     // 紀錄 爆炸的位置
        short myCurrent = 0;  // 紀錄 爆炸的圖形次序編號
        public bool alive = false;

        KinectTracker tracker;

        // 加入一個座標
        public void AddPos(Vector2 pos)
        {
            myPos = pos;
            myCurrent = 0;
            alive = true;
        }

        public int Count  // 屬性 幾張小圖
        {
            get
            {
                return ICON_List.Count;
            }
        }

        public G2D_Sprite_Explore2(Game game, KinectTracker tracker, Texture2D my, short col, short row)
            : base(game)
        {
            // TODO: Construct any child components here
            this.my = my;
            this.tracker = tracker;
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

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here
            
            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {

            if (tracker.GamingState == KinectTracker.GamingStates.KinectLightsaberFight)
            {

                if (alive == false) return;

                // TODO: Add your update code here
                ElapsedTime += gameTime.ElapsedGameTime.TotalMilliseconds; // 兩個 Frame 間隔的千分秒
                if (ElapsedTime > FrameSpeed)
                {
                    ElapsedTime = 0;

                    myCurrent++;
                    if (myCurrent >= ICON_List.Count)
                    {
                        alive = false;
                    }

                }
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (tracker.GamingState == KinectTracker.GamingStates.KinectLightsaberFight)
            {

                if (alive == false) return;

                SpriteBatch spriteBatch =
                    (SpriteBatch)Game.Services.GetService(typeof(SpriteBatch));

                if (spriteBatch == null) return;

                spriteBatch.Begin();

                Rectangle dest;
                dest.Width = (int)(ICON_Size.X * scale);
                dest.Height = (int)(ICON_Size.Y * scale);
                dest.X = (int)myPos.X - dest.Width / 2;
                dest.Y = (int)myPos.Y - dest.Height / 2;

                Rectangle src;
                src.Width = ICON_Size.X;
                src.Height = ICON_Size.Y;
                src.X = ICON_List[myCurrent].X;
                src.Y = ICON_List[myCurrent].Y;

                spriteBatch.Draw(my,   // 2D Texture
                                 dest,   // 目的區的 矩形區塊
                                 src,   // 來源區的 矩形區塊，null就是 全圖
                                 new Color(255, 255, 255, alpha), // 顏色 濾鏡 + 不透明度
                                 0,  // 旋轉徑度
                                 Vector2.Zero, // 2D 紋理圖的旋轉中心點
                                 SpriteEffects.None,  // 旋轉效果
                                 1.0f); // 圖層深度 0.0 ~ 1.0 (後)
                spriteBatch.End();
            }

            base.Draw(gameTime);
        }

    }
}