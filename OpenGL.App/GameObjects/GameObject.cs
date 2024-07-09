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

namespace OpenGL.App.GameObjects
{
    public class GameObject : IDisposable
    {
        public ProjectionTypeEnum ProjectionType;

        internal List<Texture> Textures = new List<Texture>();
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

        public GameObject()
        {
            Position = Vector3.One;
            Scale = Vector3.One;
            Rotation = Quaternion.Identity;
        }

        public GameObject(Vector3 position, Vector3 scale, Quaternion rotation, ProjectionTypeEnum projectionType, ShaderProgram shaderFactoryID, VertexBuffer[] vertexArray, int[] indices)
        {
            Position = position;
            Scale = scale;
            Rotation = rotation;
            ProjectionType = projectionType;
            ShaderProgram = shaderFactoryID;

            VertexArray = new VertexArray(vertexArray, shaderFactoryID.ShaderProgramHandle);

            IndexBuffer = new IndexBuffer(indices.Length, true);
            IndexBuffer.SetData(indices, indices.Length);
        }

        public GameObject(Vector3 position, Vector3 scale, Quaternion rotation, ProjectionTypeEnum projectionType, string shaderFactoryID, VertexBuffer[] vertexArray, int[] indices)
        {
            Position = position;
            Scale = scale;
            Rotation = rotation;
            ProjectionType = projectionType;
            ShaderFactoryID = shaderFactoryID;

            VertexArray = new VertexArray(vertexArray, ShaderFactory.ShaderPrograms[shaderFactoryID].ShaderProgramHandle);

            IndexBuffer = new IndexBuffer(indices.Length, true);
            IndexBuffer.SetData(indices, indices.Length);
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

            return ShaderFactory.ShaderPrograms[ShaderFactoryID];
        }

        public virtual void GPU_Use()
        {
            foreach (Texture.TextureData textureData in TextureDatas.Values)
            {
                foreach (var uniformKVP in textureData.SetUniformOnAdd)
                {
                    GetShaderProgram().SetUniform(uniformKVP.Key, uniformKVP.Value);
                }
            }

            GPU_Use_Shader();

            foreach (var texture in Textures)
            {
                var textureIndex = Textures.IndexOf(texture);
                var textureData = TextureDatas[textureIndex];
                texture.GPU_Use(textureData);
            }

            GPU_Use_Vertex();
        }


        internal virtual void GPU_Use_Shader()
        {
            GetShaderProgram().Use(); //Use shader program
        }

        internal virtual void GPU_Use_Vertex()
        {
            GL.BindVertexArray(VertexArray.VertexArrayHandle); //Use vertex array handle to grab the vec3's variable

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBuffer.IndexBufferHandle); //Use indices array linked to vertex array above

            GL.DrawElements(PrimitiveType.Triangles, IndexBuffer.IndexCount, DrawElementsType.UnsignedInt, 0); //Tell it to draw with the indices array
        }

        public virtual void Update(FrameEventArgs args)
        {

        }

        public void Dispose()
        {
            //Removing everything
            foreach (var texture in Textures)
            {
                texture.Dispose();
            }
            VertexArray?.Dispose();
            IndexBuffer?.Dispose();
        }
    }
}
