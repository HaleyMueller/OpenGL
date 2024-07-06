using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenGL.App.Core;
using OpenGL.App.Core.Shader;
using OpenGL.App.Core.Texture;
using OpenGL.App.Core.UniformBufferObject;
using OpenGL.App.Core.Vertex;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static OpenGL.App.Core.TileFactory;
using static OpenGL.App.Core.UniformBufferObject.UniformBufferObjectFactory;

namespace OpenGL.App
{
    public class GameObject : IDisposable
    {
        public ProjectionTypeEnum ProjectionType;

        private List<Texture> Textures = new List<Texture>();
        public string ShaderFactoryID;
        public VertexArray VertexArray;
        public IndexBuffer IndexBuffer;

        public Vector3 Position;
        public Vector3 Scale;
        public Quaternion Rotation;

        public ShaderProgram ShaderProgram;

        public Matrix4 ModelView
        {
            get
            {
                return Matrix4.CreateFromQuaternion(Rotation) * Matrix4.CreateScale(Scale) * Matrix4.CreateTranslation(Position);
            }
        }

        public enum ProjectionTypeEnum
        {
            Perspective,
            Orthographic
        }

        public Dictionary<int, Texture.TextureData> TextureDatas = new Dictionary<int, Texture.TextureData>();

        public GameObject(Vector3 position, Vector3 scale, Quaternion rotation, ProjectionTypeEnum projectionType, ShaderProgram shaderFactoryID, VertexBuffer[] vertexArray, int[] indices)
        {
            Position = position;
            Scale = scale;
            Rotation = rotation;
            ProjectionType = projectionType;
            ShaderProgram = shaderFactoryID;

            VertexArray = new VertexArray(vertexArray, shaderFactoryID.ShaderProgramHandle);

            this.IndexBuffer = new IndexBuffer(indices.Length, true);
            this.IndexBuffer.SetData(indices, indices.Length);
        }

        public GameObject(Vector3 position, Vector3 scale, Quaternion rotation, ProjectionTypeEnum projectionType, string shaderFactoryID, VertexBuffer[] vertexArray, int[] indices)
        {
            Position = position;
            Scale = scale;
            Rotation = rotation;
            ProjectionType = projectionType;
            ShaderFactoryID = shaderFactoryID;

            VertexArray = new VertexArray(vertexArray, ShaderFactory.ShaderPrograms[shaderFactoryID].ShaderProgramHandle);

            this.IndexBuffer = new IndexBuffer(indices.Length, true);
            this.IndexBuffer.SetData(indices, indices.Length);
        }

        public void AddTexture(Texture texture, Texture.TextureData data)
        {
            int i = Textures.Count;

            Textures.Add(texture);
            TextureDatas.Add(i, data);

            foreach (var uniformKVP in data.SetUniformOnAdd)
            {
                GetShaderProgram().SetUniform(uniformKVP.Key, uniformKVP.Value);
            }
        }

        public ShaderProgram GetShaderProgram()
        {
            if (ShaderProgram != null) 
                return ShaderProgram;

            return ShaderFactory.ShaderPrograms[this.ShaderFactoryID];
        }

        public void GPU_Use()
        {
            foreach (Texture.TextureData textureData in this.TextureDatas.Values)
            {
                foreach (var uniformKVP in textureData.SetUniformOnAdd)
                {
                    GetShaderProgram().SetUniform(uniformKVP.Key, uniformKVP.Value);
                }
            }

            GPU_Use_Shader();

            foreach (var texture in  this.Textures)
            {
                var textureIndex = this.Textures.IndexOf(texture);
                var textureData = TextureDatas[textureIndex];
                texture.GPU_Use(textureData);
            }

            GPU_Use_Vertex();
        }


        internal virtual void GPU_Use_Shader()
        {
            GetShaderProgram().Use(); //Use shader program
        }

        private void GPU_Use_Vertex()
        {
            GL.BindVertexArray(this.VertexArray.VertexArrayHandle); //Use vertex array handle to grab the vec3's variable

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.IndexBuffer.IndexBufferHandle); //Use indices array linked to vertex array above

            GL.DrawElements(PrimitiveType.Triangles, this.IndexBuffer.IndexCount, DrawElementsType.UnsignedInt, 0); //Tell it to draw with the indices array
        }

        public virtual void Update(FrameEventArgs args)
        {

        }

        public void Dispose()
        {
            //Removing everything
            foreach (var texture in this.Textures)
            {
                texture.Dispose();
            }
            this.VertexArray?.Dispose();
            this.IndexBuffer?.Dispose();
        }
    }

    public class PlaneWithImage : GameObject
    {
        public PlaneWithImage(Vector3 position, Vector3 scale, Quaternion rotation, ProjectionTypeEnum projectionType, ShaderProgram shaderFile, VertexBuffer[] vertexArray, int[] indices) : base(position, scale, rotation, projectionType, shaderFile, vertexArray, indices)
        {

        }

        public PlaneWithImage(Vector3 position, Vector3 scale, Quaternion rotation, ProjectionTypeEnum projectionType, string shaderFile, VertexBuffer[] vertexArray, int[] indices) : base(position, scale, rotation, projectionType, shaderFile, vertexArray, indices)
        {

        }

        public float colorFactor = 1f;
        public bool flipColor = false;

        Resources.Shaders.VertexColor[] verticesColor = new Resources.Shaders.VertexColor[]
        {
            new Resources.Shaders.VertexColor(new Color4(1f, 0f, 0f, 1f)),
            new Resources.Shaders.VertexColor(new Color4(0f, 1f, 0f, 1f)),
            new Resources.Shaders.VertexColor(new Color4(0f, 0f, 1f, 1f)),
            new Resources.Shaders.VertexColor(new Color4(1f, 1f, 1f, 1f))
        };

        internal override void GPU_Use_Shader()
        {
            GetShaderProgram().SetUniform("model", this.ModelView);
            base.GPU_Use_Shader();
        }

        public void Update(FrameEventArgs args)
        {
            if (colorFactor >= 1f)
            {
                flipColor = false;
            }
            else if (colorFactor <= 0f)
            {
                flipColor = true;
            }

            if (flipColor)
            {
                colorFactor += (float)(0.4f * args.Time);
            }
            else
            {
                colorFactor -= (float)(0.4f * args.Time);
            }

            //Console.WriteLine($"ColorFactor:{colorFactor}");

            var copiedVertColor = new Resources.Shaders.VertexColor[verticesColor.Length];
            Array.Copy(verticesColor, copiedVertColor, verticesColor.Length);

            for (int i = 0; i < copiedVertColor.Length; i++)
            {
                copiedVertColor[i].Color.R *= colorFactor;
                copiedVertColor[i].Color.G *= colorFactor;
                copiedVertColor[i].Color.B *= colorFactor;
            }

            //base.VertexArray.VertexBuffer["Color"].SetData(copiedVertColor, copiedVertColor.Length);
        }
    }

    public class Tile : GameObject
    {
        private int TileID;
        private Texture.TextureData TextureData;

        public Tile(Vector3 position, Vector3 scale, Quaternion rotation, ProjectionTypeEnum projectionType, string shaderFactoryID, VertexBuffer[] vertexArray, int[] indices) : base(position, scale, rotation, projectionType, shaderFactoryID, vertexArray, indices)
        {
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
            GetShaderProgram().SetUniform("model", this.ModelView);

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
