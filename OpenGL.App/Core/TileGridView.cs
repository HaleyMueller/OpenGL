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

        public void DecreaseLayer()
        {
            CurrentLayer -= 1;

            if (CurrentLayer < 0)
            {
                CurrentLayer = 0;
            }
        }

        public void IncreaseLayer()
        {
            CurrentLayer += 1;

            if (CurrentLayer >= tileGridLayers.Length)
            {
                CurrentLayer = tileGridLayers.Length -1;
            }
        }

        //Maybe create a new tilegrid only with visible objects instead of each layer having a tilegrid?
        public int[,,] VisibleTiles()
        {
            var ret = new int[3,3,3];


            return ret;
        }

        public void GPU_Use()
        {
            for (int i = CurrentLayer; i >= 0; i--) //Does this layer have a transparent tile?
            {
                tileGridLayers[i].GPU_Use();

                if (tileGridLayers[i].HasATransparentTile() == false)
                    break;
            }
        }
    }
}
