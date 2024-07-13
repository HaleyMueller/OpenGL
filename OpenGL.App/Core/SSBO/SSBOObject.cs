using OpenGL.App.Core.Vertex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace OpenGL.App.Core.SSBO
{
    public class SSBOObject
    {
        public static readonly int MinVertexCount = 1;
        public static readonly int MaxVertexCount = 100_000;
        private bool IsDisposed;

        public readonly int ShaderStorageBufferHandle;
        public readonly int VertexCount;
        public readonly VertexInfo VertexInfo;
        public readonly bool IsStatic;
        public readonly string Name;

        public object Data;

        public SSBOObject(VertexInfo vertexInfo, int vertexCount, string name, SSBOFactory.SSBOIndex uboIndex, bool isStatic = true)
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

            ShaderStorageBufferHandle = GL.GenBuffer(); //Get next buffer
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ShaderStorageBufferHandle); //Tell it that this buffer is an array
            GL.BufferData(BufferTarget.ShaderStorageBuffer, VertexCount * VertexInfo.SizeInBytes, nint.Zero, hint); //Se how many bytes the data will be on the gpu through the buffer
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0); //Bind buffer to 0 to make sure it is done

            GL.BindBufferRange(BufferRangeTarget.ShaderStorageBuffer, (int)uboIndex, ShaderStorageBufferHandle, 0, vertexInfo.SizeInBytes);
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

            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ShaderStorageBufferHandle);
            GL.BufferSubData(BufferTarget.ShaderStorageBuffer, nint.Zero, count * VertexInfo.SizeInBytes, data);
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
        }

        public void SetData<T>(T data) where T : struct
        {
            if (typeof(T) != VertexInfo.Type)
            {
                throw new ArgumentException("Generic type 'T' does not match the vertex type of the vertex buffer.");
            }

            Data = data;

            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, ShaderStorageBufferHandle);
            GL.BufferSubData(BufferTarget.ShaderStorageBuffer, nint.Zero, VertexInfo.SizeInBytes, ref data);
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
        }

        ~SSBOObject()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
            GL.DeleteBuffer(ShaderStorageBufferHandle);

            IsDisposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
