using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace OpenGL.App
{
    public readonly struct VertexAttribute
    {
        public readonly string Name;
        public readonly int Index; 
        public readonly int ComponentCount;
        public readonly int Offset;

        public VertexAttribute(string name, int index, int componentCount, int offset)
        {
            Name = name;
            Index = index;
            ComponentCount = componentCount;
            Offset = offset;
        }
    }

    public sealed class VertexInfo
    {
        public readonly Type Type;
        public readonly int SizeInBytes;
        public readonly VertexAttribute[] VertexAttributes;

        public VertexInfo(Type type, params VertexAttribute[] attributes) 
        { 
            this.Type = type;
            this.SizeInBytes = 0;
            this.VertexAttributes = attributes;

            for (int i = 0; i < attributes.Length; i++)
            {
                VertexAttribute attribute = attributes[i];
                this.SizeInBytes += attribute.ComponentCount * sizeof(float);
            }
        }
    }


    /// <summary>
    /// When we send the array of VertexPositionColor the graphics card just sees a large array of floats (6 floats per vertex) it doesn't see the surrounding struct.. just the base data or array of floats.
    /// </summary>
    public  struct VertexPositionColor
    {
        public  Vector2 Position;
        public readonly Color4 Color;

        public static readonly VertexInfo VertexInfo = 
        new VertexInfo
        (
            typeof(VertexPositionColor), 
            new VertexAttribute("Position", 0, 2, 0),
            new VertexAttribute("Color", 2, 4, 4 * sizeof(float))
        );

        public VertexPositionColor(Vector2 position, Color4 color)
        {
            this.Position = position;
            this.Color = color;
        }
    }

    public struct VertexPosition
    {
        public Vector2 Position;

        public static readonly VertexInfo VertexInfo =
        new VertexInfo
        (
            typeof(VertexPosition),
            new VertexAttribute("Position", 0, 2, 0)
        );

        public VertexPosition(Vector2 position)
        {
            this.Position = position;
        }
    }

    public interface  test
    {

    }

    public struct VertexColor : test
    {
        public Color4 Color;

        public static readonly VertexInfo VertexInfo =
        new VertexInfo
        (
            typeof(VertexColor),
            new VertexAttribute("Color", 2, 4, 0) //Offset is dependent on this list of attributes only. Not the total going into the VAO
        );

        public VertexColor(Color4 position)
        {
            this.Color = position;
        }
    }

    public readonly struct VertexPositionTexture
    {
        public readonly Vector2 Position;
        public readonly Vector2 TexCoord;

        public static readonly VertexInfo VertexInfo = 
        new VertexInfo
        (
            typeof(VertexPositionTexture),
            new VertexAttribute("Position", 0, 2, 0),
            new VertexAttribute("TexCoord", 1, 2, 2 * sizeof(float))
        );

        public VertexPositionTexture(Vector2 position, Vector2 texCoord)
        {
            this.Position = position;
            this.TexCoord = texCoord;
        }

    }

    public readonly struct VertexPositionTextureColor
    {
        public readonly Vector2 Position;
        public readonly Vector2 TexCoord;
        public readonly Color4 Color;

        public static readonly VertexInfo VertexInfo =
        new VertexInfo
        (
            typeof(VertexPositionTextureColor),
            new VertexAttribute("Position", 0, 2, 0),
            new VertexAttribute("TexCoord", 1, 2, 2 * sizeof(float)), //TODO automatically create offsets
            new VertexAttribute("Color", 2, 4, 4 * sizeof(float))
        );

        public VertexPositionTextureColor(Vector2 position, Vector2 texCoord, Color4 color)
        {
            this.Position = position;
            this.TexCoord = texCoord;
            this.Color = color;
        }

    }
}
