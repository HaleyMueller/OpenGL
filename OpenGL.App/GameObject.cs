using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

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

        public virtual void Update(FrameEventArgs args)
        {

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

    public class PlaneWithImage : GameObject
    {
        public PlaneWithImage(Vector3 position, ProjectionTypeEnum projectionType, string shaderFile, VertexBuffer[] vertexArray, int[] indices, Texture2D texture = null) : base(position, projectionType, shaderFile, vertexArray, indices, texture)
        {

        }

        public float colorFactor = 1f;
        public bool flipColor = false;

        VertexColor[] verticesColor = new VertexColor[]
        {
            new VertexColor(new Color4(1f, 0f, 0f, 1f)),
            new VertexColor(new Color4(0f, 1f, 0f, 1f)),
            new VertexColor(new Color4(0f, 0f, 1f, 1f)),
            new VertexColor(new Color4(1f, 1f, 1f, 1f))
        };

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

            var copiedVertColor = new VertexColor[verticesColor.Length];
            Array.Copy(verticesColor, copiedVertColor, verticesColor.Length);

            for (int i = 0; i < copiedVertColor.Length; i++)
            {
                copiedVertColor[i].Color.R *= colorFactor;
                copiedVertColor[i].Color.G *= colorFactor;
                copiedVertColor[i].Color.B *= colorFactor;
            }

            base.VertexArray.VertexBuffer[1].SetData(copiedVertColor, copiedVertColor.Length);
        }
    }
}
