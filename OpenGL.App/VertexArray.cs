using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace OpenGL.App
{
    /// <summary>
    /// This class gives the gpu the information you need for a point (vertex on the screen). Such as: position, color, textures, etc.
    /// </summary>
    public sealed class VertexArray : IDisposable
    {
        public bool disposed;

        public readonly int VertexArrayHandle;
        public readonly VertexBuffer[] VertexBuffer;

        public VertexArray(VertexBuffer[] vertexBuffer) 
        {
            this.disposed = false;

            if (vertexBuffer is null)
            {
                throw new ArgumentNullException(nameof(vertexBuffer));
            }

            this.VertexBuffer = vertexBuffer;

            this.VertexArrayHandle = GL.GenVertexArray();
            GL.BindVertexArray(this.VertexArrayHandle);

            foreach (var vbo in vertexBuffer)
            {
                int vertexSizeInBytes = vbo.VertexInfo.SizeInBytes;
                VertexAttribute[] attributes = vbo.VertexInfo.VertexAttributes;

                //These 3 lines are going into the BindVertexArray and saving it
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo.VertexBufferHandle); //Grab the array that we saved into gpu from the load

                for (int i = 0; i < attributes.Length; i++)
                {
                    var attribute = attributes[i];
                    GL.VertexAttribPointer(attribute.Index, attribute.ComponentCount, VertexAttribPointerType.Float, false, vertexSizeInBytes, attribute.Offset); //Fill in the value from vertexShaderCode by location id. Also how to define each byte segment from the array
                    GL.EnableVertexAttribArray(attribute.Index); //Enable that variable location id on the shader
                }
            }

            GL.BindVertexArray(0);
        }

        ~VertexArray()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (this.disposed) return;

            GL.BindVertexArray(0);
            GL.DeleteVertexArray(this.VertexArrayHandle);

            this.disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
