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

        private int[] Indices = new int[]
        {
            0, 1, 2, 0, 2, 3
        };

        public TileGrid(int w, int h) : base()
        {
            Width = w;
            Height = h;

            base.ShaderFactoryID = "TileInstanced.glsl";

            var vertices = ModelVertices();

            var vertexBuffer = new VertexBuffer(Resources.Shaders.VertexPositionTexture.VertexInfo, vertices.Length, "PositionAndTexture", true);
            vertexBuffer.SetData(vertices, vertices.Length);

            var offsets = OffsetPosition();
            var offsetVertexBuffer = new VertexBuffer(Resources.Shaders.TileInstanced.VertexInfo, Width * Height, "Offsets");
            offsetVertexBuffer.SetData(offsets);

            VertexArray = new VertexArray(new VertexBuffer[] { vertexBuffer, offsetVertexBuffer }, GetShaderProgram().ShaderProgramHandle);

            IndexBuffer = new IndexBuffer(Indices.Length, true);
            IndexBuffer.SetData(Indices, Indices.Length);

            SetTileID(1);

            TileGameObjects = new TileGameObject[w, h];
            
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    TileGameObjects[i, j] = new TileGameObject(i, j);
                    //TileGameObjects[i, j].SetTileID(1);
                }
            }
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

        private Resources.Shaders.TileInstanced[] OffsetPosition()
        {
            var vertices = new Resources.Shaders.TileInstanced[Width*Height];

            int index = 0;
            float offset = .1f;
            for (int y = -Height; y < Height; y += 2)
            {
                for (int x = -Width; x < Width; x += 2)
                {
                    Vector2 translation = new Vector2();
                    translation.X = (float)x / Width + offset;
                    translation.Y = (float)y / Height + offset;
                    vertices[index++] = new Resources.Shaders.TileInstanced(translation);
                }
            }

            return vertices;
        }

        public override void GPU_Use()
        {
            foreach (Texture.Texture.TextureData textureData in TextureDatas.Values)
            {
                foreach (var uniformKVP in textureData.SetUniformOnAdd)
                {
                    GetShaderProgram().SetUniform(uniformKVP.Key, uniformKVP.Value);
                }
            }

            GPU_Use_Shader();

            GPU_Use_Vertex();
        }

        internal override void GPU_Use_Shader()
        {
            GetShaderProgram().SetUniform("model", ModelView);
            if (Game._Game.IsBindlessSupported) //Techincally not needed if we did program bindless lookup for bindless textures
            {
                base.GPU_Use_Shader();
                Game._Game.TileTextureFactory.GPU_Use(1, GetShaderProgram(), TextureData);
            }
            else
            {
                Game._Game.TileTextureFactory.GPU_Use(1, GetShaderProgram(), TextureData);
                base.GPU_Use_Shader();
            }
        }
        TextureData TextureData { get; set; }

        public void SetTileID(int tileID)
        {
            TextureData = new TextureData() { SelectedTexture = tileID };

            if (Game._Game.IsBindlessSupported)
            {
                TextureData.ShaderUniformLocation = GetShaderProgram().GetUniform("bindlessTexture").Location;
            }
            else
            {
                GetShaderProgram().SetUniform("selectedTexture", tileID);
            }
        }

        internal override void GPU_Use_Vertex()
        {
            GL.BindVertexArray(VertexArray.VertexArrayHandle); //Use vertex array handle to grab the vec3's variable

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBuffer.IndexBufferHandle); //Use indices array linked to vertex array above

            GL.DrawElementsInstanced(PrimitiveType.Triangles, IndexBuffer.IndexCount, DrawElementsType.UnsignedInt, 0, Width*Height); //Tell it to draw with the indices array
        }

        public void Dispose()
        {
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    TileGameObjects[i, j].Dispose();
                }
            }
        }
    }
}
