using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace OpenGL.App
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

        public VertexAttribute(string name, int componentCount, int sizeofType)
        {
            Name = name;
            ComponentCount = componentCount;
            SizeOfType = sizeofType;
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
                this.SizeInBytes += attribute.ComponentCount * attribute.SizeOfType;
            }
        }
    }


    /// <summary>
    /// When we send the array of VertexPositionColor the graphics card just sees a large array of floats (6 floats per vertex) it doesn't see the surrounding struct.. just the base data or array of floats.
    /// </summary>
    public  struct VertexPositionColor
    {
        public Vector2 Position;
        public readonly Color4 Color;

        public static readonly VertexInfo VertexInfo = 
        new VertexInfo
        (
            typeof(VertexPositionColor), 
            new VertexAttribute("Position", 2, sizeof(float)),
            new VertexAttribute("Color", 4, sizeof(float))
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
            new VertexAttribute("Position", 2, sizeof(float))
        );

        public VertexPosition(Vector2 position)
        {
            this.Position = position;
        }
    }

    public interface VertexDefinition
    {

    }

    public class VertexDefinitionGroup
    {
        public List<VertexDefinition> VertexDefinitions = new List<VertexDefinition>();
    }

    public struct VertexColor : VertexDefinition
    {
        public Color4 Color;

        public static readonly VertexInfo VertexInfo =
        new VertexInfo
        (
            typeof(VertexColor),
            new VertexAttribute("Color", 4, sizeof(float)) //Offset is dependent on this list of attributes only. Not the total going into the VAO
        );

        public VertexColor(Color4 position)
        {
            this.Color = position;
        }
    }

    public readonly struct VertexPositionTexture : VertexDefinition
    {
        public readonly Vector2 Position;
        public readonly Vector2 TexCoord;

        public static readonly VertexInfo VertexInfo = 
        new VertexInfo
        (
            typeof(VertexPositionTexture),
            new VertexAttribute("Position", 2, sizeof(float)),
            new VertexAttribute("TexCoord", 2, sizeof(float))
        );

        public VertexPositionTexture(Vector2 position, Vector2 texCoord)
        {
            this.Position = position;
            this.TexCoord = texCoord;
        }

    }

    public readonly struct VertexPositionTextureArray : VertexDefinition
    {
        public readonly VertexPositionTexture[] Array;


        public VertexPositionTextureArray(VertexPositionTexture[] array)
        {
            this.Array = array;
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
            new VertexAttribute("Position", 2, sizeof(float)),
            new VertexAttribute("TexCoord", 2, sizeof(float)),
            new VertexAttribute("Color", 4, sizeof(float))
        );

        public VertexPositionTextureColor(Vector2 position, Vector2 texCoord, Color4 color)
        {
            this.Position = position;
            this.TexCoord = texCoord;
            this.Color = color;
        }

    }
}
