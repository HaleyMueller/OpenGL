using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace OpenGL.App.Core.Vertex
{
    /// <summary>
    /// This class will set up how big the information you are setting on the gpu is that will be used for the vertex array
    /// </summary>
    public sealed class VertexBuffer : IDisposable
    {
        public static readonly int MinVertexCount = 1;
        public static readonly int MaxVertexCount = 100_000;
        private bool IsDisposed;

        public readonly int VertexBufferHandle;
        public readonly int VertexCount;
        public readonly VertexInfo VertexInfo;
        public readonly bool IsStatic;
        public readonly string Name;

        public Array Data;

        public VertexBuffer(VertexInfo vertexInfo, int vertexCount, string name, bool isStatic = true)
        {
            IsDisposed = false;

            if (vertexCount < MinVertexCount || vertexCount > MaxVertexCount)
            {
                throw new ArgumentOutOfRangeException(nameof(vertexCount));
            }

            VertexInfo = vertexInfo;
            VertexCount = vertexCount;
            IsStatic = isStatic;
            Name = name;

            BufferUsageHint hint = BufferUsageHint.StaticDraw; //This will tell the buffer that the info we give it wont change on the cpu;

            if (!IsStatic)
            {
                hint = BufferUsageHint.StreamDraw; //This will tell the buffer that the info we give it WILL change on the cpu at some point;
            }

            //int vertexSizeInBytes = VertexPositionColor.VertexInfo.SizeInBytes;

            VertexBufferHandle = GL.GenBuffer(); //Get next buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferHandle); //Tell it that this buffer is an array
            GL.BufferData(BufferTarget.ArrayBuffer, VertexCount * VertexInfo.SizeInBytes, nint.Zero, hint); //Se how many bytes the data will be on the gpu through the buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0); //Bind buffer to 0 to make sure it is done
        }

        //This sets the data of the vertex handle, which means you can edit the data in real time to move stuff
        public void SetData<T>(T[] data) where T : struct
        {
            SetData(data, data.Length);
        }

        public void SetData<T>(T[] data, int count) where T : struct
        {
            if (typeof(T) != VertexInfo.Type)
            {
                throw new ArgumentException("Generic type 'T' does not match the vertex type of the vertex buffer.");
            }

            if (data is null)
            {
                throw new ArgumentOutOfRangeException(nameof(data));
            }

            if (count <= 0 ||
                count > VertexCount ||
                count > data.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            Data = data;

            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferHandle);
            GL.BufferSubData(BufferTarget.ArrayBuffer, nint.Zero, count * VertexInfo.SizeInBytes, data);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        ~VertexBuffer()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DeleteBuffer(VertexBufferHandle);

            IsDisposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
