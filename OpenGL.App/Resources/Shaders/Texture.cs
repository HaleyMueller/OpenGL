using OpenGL.App.Core.Vertex;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenGL.App.Resources.Shaders
{
    public readonly struct VertexPositionTexture
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
            Position = position;
            TexCoord = texCoord;
        }
    }
}
