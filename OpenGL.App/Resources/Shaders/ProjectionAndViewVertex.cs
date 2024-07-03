using OpenGL.App.Core.Vertex;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenGL.App.Resources.Shaders
{
    public struct VertexProjectionView
    {
        public Matrix4 Projection;
        public Matrix4 View;

        public static readonly VertexInfo VertexInfo =
        new VertexInfo
        (
            typeof(VertexProjectionView),
            new VertexAttribute("Projection", 16, sizeof(float)),
            new VertexAttribute("View", 16, sizeof(float)) //Offset is dependent on this list of attributes only. Not the total going into the VAO
        );

        public VertexProjectionView(Matrix4 projection, Matrix4 view)
        {
            Projection = projection;
            View = view;
        }
    }
}
