using OpenGL.App.Core.Shader;
using OpenGL.App.Core.Texture;
using OpenGL.App.Core.Vertex;
using OpenGL.App.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace OpenGL.App.GameObjects
{
    public class TileGameObject : GameObject
    {
        private int TileID;
        private Texture.TextureData TextureData;

        private int GridPosX;
        private int GridPosY;

        private int[] Indices = new int[]
        {
            0, 1, 2, 0, 2, 3
        };

        private Resources.Shaders.VertexPositionTexture[] ModelVertices()
        {
            var vertices = new Resources.Shaders.VertexPositionTexture[4];

            vertices[0] = new Resources.Shaders.VertexPositionTexture(new Vector2(0.5f, 0.5f), new Vector2(1, 1));
            vertices[1] = new Resources.Shaders.VertexPositionTexture(new Vector2(0.5f, -0.5f), new Vector2(1, 0));
            vertices[2] = new Resources.Shaders.VertexPositionTexture(new Vector2(-0.5f, -0.5f), new Vector2(0, 0));
            vertices[3] = new Resources.Shaders.VertexPositionTexture(new Vector2(-0.5f, 0.5f), new Vector2(0, 1));

            return vertices;
        }

        private void UpdateTilePos(int gridPosX, int gridPosY)
        {
            GridPosX = gridPosX;
            GridPosY = gridPosY;
            Position = new Vector3(gridPosX*.1f, gridPosY * .1f, 0);
        }

        public TileGameObject() : base()
        {
            Scale = new Vector3(0.1f, 0.1f, 0.1f);

            ShaderProgram = ShaderFactory.ShaderPrograms["Tile.glsl"];

            UpdateTilePos(0, 0);

            var vertices = ModelVertices();

            var vertexBuffer = new VertexBuffer(Resources.Shaders.VertexPositionTexture.VertexInfo, vertices.Length, "PositionAndTexture", true);
            vertexBuffer.SetData(vertices, vertices.Length);

            VertexArray = new VertexArray(new VertexBuffer[] { vertexBuffer }, GetShaderProgram().ShaderProgramHandle);

            IndexBuffer = new IndexBuffer(Indices.Length, true);
            IndexBuffer.SetData(Indices, Indices.Length);
        }

        public TileGameObject(int gridPosX, int gridPosY) : base()
        {
            Scale = new Vector3(0.1f, 0.1f, 0.1f);

            UpdateTilePos(gridPosX, gridPosY);

            //var vertices = ModelVertices();

            //var vertexBuffer = new VertexBuffer(Resources.Shaders.VertexPositionTexture.VertexInfo, vertices.Length, "PositionAndTexture", true);
            //vertexBuffer.SetData(vertices, vertices.Length);

            //VertexArray = new VertexArray(new VertexBuffer[] { vertexBuffer }, GetShaderProgram().ShaderProgramHandle);

            //IndexBuffer = new IndexBuffer(Indices.Length, true);
            //IndexBuffer.SetData(Indices, Indices.Length);
        }

        public void SetTileID(int tileID)
        {
            TileID = tileID;

            TextureData = new Texture.TextureData() { SelectedTexture = tileID };

            if (Game._Game.IsBindlessSupported)
            {
                TextureData.ShaderUniformLocation = GetShaderProgram().GetUniform("bindlessTexture").Location;
            }
            else
            {
                GetShaderProgram().SetUniform("selectedTexture", tileID);
            }
        }

        public int GetileID()
        {
            return TileID;
        }

        internal override void GPU_Use_Shader()
        {
            GetShaderProgram().SetUniform("model", ModelView);

            if (Game._Game.IsBindlessSupported) //Techincally not needed if we did program bindless lookup for bindless textures
            {
                base.GPU_Use_Shader();
                Game._Game.TileTextureFactory.GPU_Use(TileID, GetShaderProgram(), TextureData);
            }
            else
            {
                Game._Game.TileTextureFactory.GPU_Use(TileID, GetShaderProgram(), TextureData);
                base.GPU_Use_Shader();
            }

        }
    }
}
