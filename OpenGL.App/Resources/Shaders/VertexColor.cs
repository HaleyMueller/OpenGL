using OpenGL.App.Core.Vertex;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenGL.App.Resources.Shaders
{
    public struct VertexColor
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
            Color = position;
        }
    }
}
