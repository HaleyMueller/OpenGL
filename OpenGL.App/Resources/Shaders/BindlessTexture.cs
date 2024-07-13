using OpenGL.App.Core.Vertex;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenGL.App.Resources.Shaders
{
    public struct BindlessTexture
    {
        public long TextureID;

        public static readonly VertexInfo VertexInfo =
        new VertexInfo
        (
            typeof(BindlessTexture),
            new VertexAttribute("TextureID", 1, sizeof(long))
        );

        public BindlessTexture(long textureID)
        {
            TextureID = textureID;
        }
    }
}
