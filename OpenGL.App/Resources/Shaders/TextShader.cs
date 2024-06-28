using OpenGL.App.Core.Vertex;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenGL.App.Resources.Shaders
{
    public struct TextShader
    {
        public Vector2 Position;
        public Vector2 TextureCoords;

        public static readonly VertexInfo VertexInfo =
        new VertexInfo
        (
            typeof(TextShader),
            new VertexAttribute("Position", 2, sizeof(float)),
            new VertexAttribute("TextureCoords", 2, sizeof(float))
        );

        public TextShader(Vector2 position, Vector2 textureCoords)
        {
            this.Position = position;
            this.TextureCoords = textureCoords;
        }
    }
}
