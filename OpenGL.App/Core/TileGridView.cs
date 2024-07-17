using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenGL.App.Core
{
    /// <summary>
    /// Top level class. This is where the data for the tile is going to be used for telling what tiles to show on screen using the current camera layer
    /// </summary>
    public class TileGridView
    {
        public TileGridLayer[] tileGridLayers;

        public int CurrentLayer { get; private set; }

        public TileGridView(int currentLayer)
        {
            CurrentLayer = currentLayer;
        }

        public int[,,] VisibleTiles()
        {
            var ret = new int[3,3,3];

            return ret;
        }
    }
}
