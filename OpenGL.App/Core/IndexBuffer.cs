using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace OpenGL.App.Core
{
    /// <summary>
    /// This tells the gpu how the to draw the indices where it won't double up vertices. (A box is 2 triangles. You don't have to draw the 2 middle vertices twice)
    /// </summary>
    public sealed class IndexBuffer : IDisposable
    {
        public static readonly int MinIndexCount = 1;
        public static readonly int MaxIndexCount = 250_000;

        private bool disposed;

        public readonly int IndexBufferHandle;
        public readonly int IndexCount;
        public readonly bool IsStatic;

        public IndexBuffer(int indexCount, bool isStatic = true)
        {
            if (indexCount < MinIndexCount ||
                indexCount > MaxIndexCount)
            {
                throw new ArgumentOutOfRangeException(nameof(indexCount));
            }

            IndexCount = indexCount;
            IsStatic = isStatic;

            BufferUsageHint hint = BufferUsageHint.StaticDraw; //This will tell the buffer that the info we give it wont change on the cpu;

            if (!IsStatic)
            {
                hint = BufferUsageHint.StreamDraw; //This will tell the buffer that the info we give it WILL change on the cpu at some point;
            }

            IndexBufferHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBufferHandle); //Tell GL that this buffer is going to be an index buffer
            GL.BufferData(BufferTarget.ElementArrayBuffer, IndexCount * sizeof(int), nint.Zero, hint); //Bind variable to buffer
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0); //Unbind
        }

        ~IndexBuffer()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (disposed) return;

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.DeleteBuffer(IndexBufferHandle);

            disposed = true;
            GC.SuppressFinalize(this);
        }

        public void SetData(int[] data, int count)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (data.Length <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(data));
            }

            if (count <= 0 ||
                count > IndexCount ||
                count > data.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBufferHandle);
            GL.BufferSubData(BufferTarget.ElementArrayBuffer, nint.Zero, count * sizeof(int), data);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }
    }
}
