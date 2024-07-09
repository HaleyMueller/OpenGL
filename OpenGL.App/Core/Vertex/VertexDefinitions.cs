using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace OpenGL.App.Core.Vertex
{
    public readonly struct VertexAttribute
    {
        /// <summary>
        /// Needs to match the shader Attribute by name
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// How many variables inside this variable. Example: Color4 is 4 floats
        /// </summary>
        public readonly int ComponentCount;
        /// <summary>
        /// What data type is this variable and what is the sizeof(it)
        /// </summary>
        public readonly int SizeOfType;

        public readonly int? VertexAttribDivisorCount;

        public VertexAttribute(string name, int componentCount, int sizeofType, int? vertexAttribDivisorCount = null)
        {
            Name = name;
            ComponentCount = componentCount;
            SizeOfType = sizeofType;
            VertexAttribDivisorCount = vertexAttribDivisorCount;
        }
    }

    public sealed class VertexInfo
    {
        public readonly Type Type;
        public readonly int SizeInBytes;
        public readonly VertexAttribute[] VertexAttributes;

        public VertexInfo(Type type, params VertexAttribute[] attributes)
        {
            Type = type;
            SizeInBytes = 0;
            VertexAttributes = attributes;

            for (int i = 0; i < attributes.Length; i++)
            {
                VertexAttribute attribute = attributes[i];
                SizeInBytes += attribute.ComponentCount * attribute.SizeOfType;
            }
        }
    }
}
