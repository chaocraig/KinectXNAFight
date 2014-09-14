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


namespace PongImproved
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class G3DBox : Microsoft.Xna.Framework.DrawableGameComponent
    {
        private VertexBuffer vertexBuffer;  // 頂點緩衝區
        private BasicEffect effect;         // 基本 特效

        private IndexBuffer indexBuffer;  // 頂點索引緩衝區
        public Matrix world, view;  // 世界矩陣、觀測矩陣
        Game game;

        public G3DBox(Game game)
            : base(game)
        {
            // TODO: Construct any child components here
            this.game = game;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here
            effect = new BasicEffect(game.GraphicsDevice);

            VertexPositionColor[] vertices = new VertexPositionColor[8];
            // 後面四個點
            vertices[0] = new VertexPositionColor(new Vector3(-1.0f, -1.0f, -1.0f), Color.Red);
            vertices[1] = new VertexPositionColor(new Vector3(-1.0f, 1.0f, -1.0f), Color.Red);
            vertices[2] = new VertexPositionColor(new Vector3(1.0f, 1.0f, -1.0f), Color.Red);
            vertices[3] = new VertexPositionColor(new Vector3(1.0f, -1.0f, -1.0f), Color.Red);

            // 前面四個點
            vertices[4] = new VertexPositionColor(new Vector3(-1.0f, -1.0f, 1.0f), Color.Red);
            vertices[5] = new VertexPositionColor(new Vector3(-1.0f, 1.0f, 1.0f), Color.Red);
            vertices[6] = new VertexPositionColor(new Vector3(1.0f, 1.0f, 1.0f), Color.Red);
            vertices[7] = new VertexPositionColor(new Vector3(1.0f, -1.0f, 1.0f), Color.Red);

            vertexBuffer = new VertexBuffer(game.GraphicsDevice,
                                            typeof(VertexPositionColor), vertices.Length,
                                            BufferUsage.WriteOnly);
            vertexBuffer.SetData<VertexPositionColor>(vertices);

            short[] vertexIndices = new short[36]; // 36 個 頂點索引
            vertexIndices[0] = 3; vertexIndices[1] = 2; vertexIndices[2] = 1; // 後面
            vertexIndices[3] = 3; vertexIndices[4] = 1; vertexIndices[5] = 0;

            vertexIndices[6] = 4; vertexIndices[7] = 5; vertexIndices[8] = 6; // 前面
            vertexIndices[9] = 4; vertexIndices[10] = 6; vertexIndices[11] = 7;

            vertexIndices[12] = 0; vertexIndices[13] = 1; vertexIndices[14] = 5; // 左面
            vertexIndices[15] = 0; vertexIndices[16] = 5; vertexIndices[17] = 4;

            vertexIndices[18] = 7; vertexIndices[19] = 6; vertexIndices[20] = 2; // 右面
            vertexIndices[21] = 7; vertexIndices[22] = 2; vertexIndices[23] = 3;

            vertexIndices[24] = 5; vertexIndices[25] = 1; vertexIndices[26] = 2;// 上面
            vertexIndices[27] = 5; vertexIndices[28] = 2; vertexIndices[29] = 6;

            vertexIndices[30] = 0; vertexIndices[31] = 4; vertexIndices[32] = 7;// 下面
            vertexIndices[33] = 0; vertexIndices[34] = 7; vertexIndices[35] = 3;

            indexBuffer = new IndexBuffer(game.GraphicsDevice,
                                          typeof(short), vertexIndices.Length,
                                          BufferUsage.None);  // 產生 頂點索引緩衝區
            indexBuffer.SetData<short>(vertexIndices);  // 複製 頂點索引資料
            base.Initialize();
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
            // TODO: Add your update code here
            effect.World = world;  // 世界矩陣
            effect.View = view; // 視覺矩陣
            effect.Projection = Matrix.CreatePerspectiveFieldOfView(
                               MathHelper.ToRadians(45.0f),
                               1.333f, 1.0f, 100.0f); // 投影矩陣
            effect.LightingEnabled = false; // 沒設光源  所以不作燈光運算
            effect.VertexColorEnabled = true;

            game.GraphicsDevice.SetVertexBuffer(vertexBuffer);
            game.GraphicsDevice.Indices = indexBuffer;

            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.FillMode = FillMode.WireFrame;
            game.GraphicsDevice.RasterizerState = rasterizerState; 

            //  Draw the 3D axis
            foreach (EffectPass CurrentPass in effect.CurrentTechnique.Passes)
            {
                CurrentPass.Apply();

                game.GraphicsDevice.DrawIndexedPrimitives(
                  PrimitiveType.TriangleList,  // 三個點 為一個面  
                  0,    //  索引偏移値  Offset to add to each vertex index in the index buffer.
                  0,    // 頂點緩衝區 的 頂點 偏移値
                  8,    // 頂點 個數
                  0,    // 開始 的 索引 Location in the index array at which to start reading vertices
                  12    // 畫 12 個 三角面
                  );
            }
            base.Draw(gameTime);
        }
    }
}
