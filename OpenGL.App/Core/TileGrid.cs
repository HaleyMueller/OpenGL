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

        public TileGrid(int w, int h, bool isInstanced) : base()
        {
            Width = w;
            Height = h;
            IsInstanced = isInstanced;

            base.ShaderFactoryID = "TileInstanced.glsl";

            var vertices = ModelVertices();

            var vertexBuffer = new VertexBuffer(Resources.Shaders.VertexPositionTexture.VertexInfo, vertices.Length, "PositionAndTexture", true);
            vertexBuffer.SetData(vertices, vertices.Length);

            var offsets = OffsetPosition();
            var offsetVertexBuffer = new VertexBuffer(Resources.Shaders.TileInstanced.VertexInfo, Width * Height, "Offsets");
            offsetVertexBuffer.SetData(offsets);

            Tiles = TileIDs();
            TileVertexBuffer = new VertexBuffer(Resources.Shaders.TileInstancedTileID.VertexInfo, Width * Height, "TileIDs");
            TileVertexBuffer.SetData(Tiles);

            VertexArray = new VertexArray(new VertexBuffer[] { vertexBuffer, offsetVertexBuffer, TileVertexBuffer }, GetShaderProgram().ShaderProgramHandle);

            IndexBuffer = new IndexBuffer(Indices.Length, true);
            IndexBuffer.SetData(Indices, Indices.Length);

            //SetTileID(1);

            if (IsInstanced == false)
            {
                TileGameObjects = new TileGameObject[w, h];

                for (int i = 0; i < w; i++)
                {
                    for (int j = 0; j < h; j++)
                    {
                        TileGameObjects[i, j] = new TileGameObject(i, j);

                        if (i % 2 == 0 && j % 2 == 0)
                        {
                            TileGameObjects[i, j].SetTileID(1);
                        }
                        else if (i % 2 == 0)
                        {
                            TileGameObjects[i, j].SetTileID(2);
                        }
                        else
                        {
                            TileGameObjects[i, j].SetTileID(0);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Updates a tile. Make sure to call SendTiles() when done
        /// </summary>
        public void UpdateTile(int w, int h, int tileID)
        {
            int index = w * Width + h;

            var textureTileID = Game._Game.TileTextureFactory.GetTextureTileIDByTileID(tileID, GetShaderProgram(), TextureData, true);

            Tiles[index] = new Resources.Shaders.TileInstancedTileID(textureTileID);
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

        private Resources.Shaders.TileInstancedTileID[] TileIDs()
        {
            var vertices = new Resources.Shaders.TileInstancedTileID[Width * Height];

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
                        vertices[index++] = new Resources.Shaders.TileInstancedTileID(1);
                    else
                        vertices[index++] = new Resources.Shaders.TileInstancedTileID(0);
                }
            }

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
            GetShaderProgram().SetUniform("model", ModelView); //TODO this is where we need to use the right arraytexture/bindless and bind it
            if (Game._Game.IsBindlessSupported) //Techincally not needed if we did program bindless lookup for bindless textures
            {
                base.GPU_Use_Shader();
                Game._Game.TileTextureFactory.GPU_Use(1, GetShaderProgram(), TextureData, true);
            }
            else
            {
                Game._Game.TileTextureFactory.GPU_Use(3, GetShaderProgram(), TextureData, true);
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
