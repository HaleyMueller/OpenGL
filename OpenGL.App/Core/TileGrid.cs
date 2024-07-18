using OpenGL.App.Core.Vertex;
using OpenGL.App.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using static OpenGL.App.Core.Texture.Texture;
using static OpenGL.App.Core.TileFactory;
using OpenGL.App.Core.Texture;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OpenGL.App.Core
{
    public class TileGrid : GameObject
    {
        public TileGameObject[,] TileGameObjects;
        public int Width { get; private set; }
        public int Height { get; private set; }
        public bool IsInstanced { get; private set; }

        private int[] Indices = new int[]
        {
            0, 1, 2, 0, 2, 3
        };

        private Resources.Shaders.TileInstancedTileID[] Tiles;
        private VertexBuffer TileVertexBuffer;

        public int TileFactoryTextureID { get; private set; }

        public TileGrid(TileData[,] data, bool isInstanced, int tileFactoryTextureID) : base()
        {
            Width = data.GetLength(0);
            Height = data.GetLength(1);
            IsInstanced = isInstanced;
            TileFactoryTextureID = tileFactoryTextureID;

            base.ShaderFactoryID = "TileInstanced.glsl";

            var vertices = ModelVertices();

            var vertexBuffer = new VertexBuffer(Resources.Shaders.VertexPositionTexture.VertexInfo, vertices.Length, "PositionAndTexture", true);
            vertexBuffer.SetData(vertices, vertices.Length);

            var offsets = OffsetPosition();
            var offsetVertexBuffer = new VertexBuffer(Resources.Shaders.TileInstanced.VertexInfo, Width * Height, "Offsets");
            offsetVertexBuffer.SetData(offsets);



            Tiles = TileIDs(data);
            TileVertexBuffer = new VertexBuffer(Resources.Shaders.TileInstancedTileID.VertexInfo, Width * Height, "TileIDs");
            TileVertexBuffer.SetData(Tiles);

            VertexArray = new VertexArray(new VertexBuffer[] { vertexBuffer, offsetVertexBuffer, TileVertexBuffer }, GetShaderProgram().ShaderProgramHandle);

            IndexBuffer = new IndexBuffer(Indices.Length, true);
            IndexBuffer.SetData(Indices, Indices.Length);
        }

        /// <summary>
        /// Updates a tile. Make sure to call SendTiles() when done
        /// </summary>
        public void UpdateTile(int w, int h, int tileID)
        {
            int index = w * Height + h;

            var textureTileID = Game._Game.TileTextureFactory.GetTextureTileIDByTileID(tileID);

            Tiles[index] = new Resources.Shaders.TileInstancedTileID(textureTileID, tileID == -1 ? false : true);
        }

        public void UpdateTile(int w, int h, bool isVisible)
        {
            int index = w * Height + h;

            Tiles[index] = new Resources.Shaders.TileInstancedTileID(Tiles[index].TextureID, isVisible);
        }

        public void UpdateTile(TileData[,] tileDatas)
        {
            Tiles = TileIDs(tileDatas);
        }

        /// <summary>
        /// Sends the tile data to the gpu
        /// </summary>
        public void SendTiles()
        {
            TileVertexBuffer.SetData(Tiles);
        }

        private Resources.Shaders.VertexPositionTexture[] ModelVertices()
        {
            var vertices = new Resources.Shaders.VertexPositionTexture[4];

            vertices[0] = new Resources.Shaders.VertexPositionTexture(new Vector2(0.5f, 0.5f), new Vector2(1, 1));
            vertices[1] = new Resources.Shaders.VertexPositionTexture(new Vector2(0.5f, -0.5f), new Vector2(1, 0));
            vertices[2] = new Resources.Shaders.VertexPositionTexture(new Vector2(-0.5f, -0.5f), new Vector2(0, 0));
            vertices[3] = new Resources.Shaders.VertexPositionTexture(new Vector2(-0.5f, 0.5f), new Vector2(0, 1));

            return vertices;
        }

        private Resources.Shaders.TileInstancedTileID[] TileIDs(TileData[,] tileData)
        {
            var vertices = new Resources.Shaders.TileInstancedTileID[Width * Height];

            var newTileData = RotateMatrix90Degrees(tileData);

            int index = 0;
            for (int w = 0; w < Width; w++)
            {
                for (int h = 0; h < Height; h++)
                {
                    var textureTileID = Game._Game.TileTextureFactory.GetTextureTileIDByTileID(newTileData[w, h].TileID);
                    vertices[index++] = new Resources.Shaders.TileInstancedTileID(textureTileID, newTileData[w, h].IsVisible);
                }
            }

            return vertices;
        }

        static TileData[,] RotateMatrix90Degrees(TileData[,] matrix)
        {
            int rowCount = matrix.GetLength(0);
            int colCount = matrix.GetLength(1);

            TileData[,] result = new TileData[colCount, rowCount];

            for (int row = 0; row < rowCount; row++)
            {
                for (int col = 0; col < colCount; col++)
                {
                    result[col, rowCount - 1 - row] = matrix[row, col];
                }
            }

            return result;
        }

        [DebuggerDisplay("TileID = {TileID} IsVisible = {IsVisible}")]
        public class TileData
        {
            public int TileID { get; set; }
            public bool IsVisible { get; set; }
        }

        private Resources.Shaders.TileInstancedTileID[] TileIDs()
        {
            var vertices = new Resources.Shaders.TileInstancedTileID[Width * Height];

            //int index = 0;
            //float offset = 1f;
            //for (int w = 0; w < Width; w++)
            //{
            //    for (int h = 0; h < Height; h++)
            //    {
            //        Vector2 translation = new Vector2();
            //        translation.X = (float)w + offset;
            //        translation.Y = (float)h + offset;

            //        if (index % 2 == 0)
            //        {
            //            var textureTileID = Game._Game.TileTextureFactory.GetTextureTileIDByTileID(1);
            //            vertices[index++] = new Resources.Shaders.TileInstancedTileID(textureTileID);
            //        }
            //        else
            //        {
            //            var textureTileID = Game._Game.TileTextureFactory.GetTextureTileIDByTileID(0);
            //            vertices[index++] = new Resources.Shaders.TileInstancedTileID(textureTileID);
            //        }
            //    }
            //}

            return vertices;
        }

        private Resources.Shaders.TileInstanced[] OffsetPosition()
        {
            var vertices = new Resources.Shaders.TileInstanced[Width * Height];

            int index = 0;
            float offset = 1f;
            for (int w = 0; w < Width; w++)
            {
                for (int h = 0; h < Height; h++)
                {
                    Vector2 translation = new Vector2();
                    translation.X = (float)w + offset;
                    translation.Y = (float)h + offset;

                    if (index % 2 == 0)
                        vertices[index++] = new Resources.Shaders.TileInstanced(translation);
                    else
                        vertices[index++] = new Resources.Shaders.TileInstanced(translation);
                }
            }

            return vertices;
        }

        public override void GPU_Use()
        {
            if (IsInstanced)
            {
                GPU_Use_Shader();

                GPU_Use_Vertex();
            }
            else
            {
                for (int i = 0; i < Width; i++)
                {
                    for (int j = 0; j < Height; j++)
                    {
                        TileGameObjects[i, j].GPU_Use();
                    }
                }
            }
        }

        internal override void GPU_Use_Shader()
        {
            GetShaderProgram().SetUniform("model", ModelView);
            if (Game._Game.IsBindlessSupported)
            {
                base.GPU_Use_Shader();
            }
            else
            {
                Game._Game.TileTextureFactory.GPU_UseByTextureID(TileFactoryTextureID, GetShaderProgram(), TextureData);
                base.GPU_Use_Shader();
            }
        }
        TextureData TextureData { get; set; }

        internal override void GPU_Use_Vertex()
        {
            GL.BindVertexArray(VertexArray.VertexArrayHandle); //Use vertex array handle to grab the vec3's variable

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBuffer.IndexBufferHandle); //Use indices array linked to vertex array above

            GL.DrawElementsInstanced(PrimitiveType.Triangles, IndexBuffer.IndexCount, DrawElementsType.UnsignedInt, 0, Width*Height); //Tell it to draw with the indices array
        }

        public void Dispose()
        {
            if (TileGameObjects != null)
            {
                for (int i = 0; i < Width; i++)
                {
                    for (int j = 0; j < Height; j++)
                    {
                        TileGameObjects[i, j]?.Dispose();
                    }
                }
            }
        }
    }
}
