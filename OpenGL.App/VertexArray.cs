using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

        public VertexArray(VertexBuffer[] vertexBuffer, int shaderProgram) 
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

                int runningSizeInBytes = 0;
                for (int i = 0; i < attributes.Length; i++)
                {
                    var attribute = attributes[i];
                    var locationID = GL.GetAttribLocation(shaderProgram, attribute.Name);

                    if (locationID < 0)
                        throw new Exception($"Couldn't find attribute location for {attribute.Name}");

                    GL.VertexAttribPointer(locationID, attribute.ComponentCount, VertexAttribPointerType.Float, false, vertexSizeInBytes, runningSizeInBytes); //Fill in the value from vertexShaderCode by location id. Also how to define each byte segment from the array
                    runningSizeInBytes += attribute.SizeOfType * attribute.ComponentCount;
                    GL.EnableVertexAttribArray(locationID); //Enable that variable location id on the shader
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

            foreach (var vbo in this.VertexBuffer)
            {
                vbo.Dispose();
            }

            GL.BindVertexArray(0);
            GL.DeleteVertexArray(this.VertexArrayHandle);

            this.disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
