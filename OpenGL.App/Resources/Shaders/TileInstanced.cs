using OpenGL.App.Core.Vertex;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenGL.App.Resources.Shaders
{
    public readonly struct TileInstanced
    {
        public readonly Vector2 Position;

        public static readonly VertexInfo VertexInfo =
        new VertexInfo
        (
            typeof(TileInstanced),
            new VertexAttribute("aOffset", 2, sizeof(float), 1)
        );

        public TileInstanced(Vector2 position)
        {
            Position = position;
        }
    }

    public readonly struct TileInstancedTileID
    {
        public readonly float TextureID;
        public readonly float IsVisible;

        public static readonly VertexInfo VertexInfo =
        new VertexInfo
        (
            typeof(TileInstancedTileID),
            new VertexAttribute("aTextureID", 1, sizeof(float), 1),
            new VertexAttribute("aIsVisible", 1, sizeof(float), 1)
        );

        public TileInstancedTileID(float textureID, bool isVisible)
        {
            TextureID = textureID;
            IsVisible = isVisible == true ? 1 : 0;
        }
    }
}
