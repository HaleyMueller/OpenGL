using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace OpenGL.App
{
    public class GameObject : IDisposable
    {
        public Vector3 Position;
        public ProjectionTypeEnum ProjectionType;

        public Texture2D Texture;
        public ShaderProgram ShaderProgram;
        public VertexArray VertexArray;
        public IndexBuffer IndexBuffer;

        public enum ProjectionTypeEnum
        {
            Perspective,
            Orthographic
        }

        public GameObject(Vector3 position, ProjectionTypeEnum projectionType, string shaderFile, VertexBuffer[] vertexArray, int[] indices, Texture2D texture = null)
        {
            Position = position;
            ProjectionType = projectionType;
            Texture = texture;
            ShaderProgram = new ShaderProgram(shaderFile);

            VertexArray = new VertexArray(vertexArray);

            this.IndexBuffer = new IndexBuffer(indices.Length, true);
            this.IndexBuffer.SetData(indices, indices.Length);
        }

        public void GPU_Use()
        {
            GL.UseProgram(this.ShaderProgram.ShaderProgramHandle); //Use shader program
            
            if (Texture != null)
                Texture.Use();
            
            GL.BindVertexArray(this.VertexArray.VertexArrayHandle); //Use vertex array handle to grab the vec3's variable

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.IndexBuffer.IndexBufferHandle); //Use indices array linked to vertex array above

            GL.DrawElements(PrimitiveType.Triangles, this.IndexBuffer.IndexCount, DrawElementsType.UnsignedInt, 0); //Tell it to draw with the indices array
        }

        public void Dispose()
        {
            //Removing everything
            this.Texture?.Dispose();
            this.VertexArray?.Dispose();
            this.IndexBuffer?.Dispose();
            this.ShaderProgram?.Dispose();
        }
    }
}
