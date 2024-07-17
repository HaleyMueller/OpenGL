using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenGL.App.FFMPEG;

namespace OpenGL.App.Core
{
    /// <summary>
    /// Holds the data for the tile layer and instanced vbos
    /// </summary>
    public class TileGridLayer
    {
        public int Layer { get; private set; }
        public int[,] TileDataGrid { get; private set; }

        public TileGrid[] TileGrids { get; private set; }

        public TileGridLayer(int layer, int[,] tileDataGrid)
        {
            Layer = layer;
            TileDataGrid = tileDataGrid;
            SetTileGrids(tileDataGrid);
        }

        private void SetTileGrids(int[,] gridData)
        {
            //If bindless then create 1 TileGrid
            //If normal way then make sure the tile ids are in the same TileGrid. If not then you will have to make another TileGrid
            if (Game._Game.IsBindlessSupported)
            {
                TileGrids = new TileGrid[1];
                TileGrids[0] = new TileGrid(gridData, true, 0);
            }
            else
            {
                var texturesNeeded = Game._Game.TileTextureFactory.GetTextureIndicesForTileIDs(gridData);

                TileGrids = new TileGrid[texturesNeeded.Count];
                int i = 0;
                foreach (var textureID in texturesNeeded)
                {
                    int[,] newGridData = new int[gridData.GetLength(0),gridData.GetLength(1)];

                    CopyMultiDimensionalArray(gridData, newGridData);

                    int index = 0;
                    for (int w = 0; w < newGridData.GetLength(0); w++)
                    {
                        for (int h = 0; h < newGridData.GetLength(1); h++)
                        {
                            var textureTileID = Game._Game.TileTextureFactory.TileTextures[newGridData[w, h]].TextureIndex;
                            if (textureTileID != textureID)
                            {
                                newGridData[w, h] = 0;
                            }
                        }
                    }

                    TileGrids[i] = new TileGrid(newGridData, true, textureID);
                    i++;
                }
            }

            TileDataGrid = gridData;
        }

        public void GPU_Use()
        {
            foreach (var tileGrid in TileGrids)
            {
                tileGrid.GPU_Use();
            }
        }

        public void UpdateTileData(int x, int y, int ID)
        {
            //If bindless then use first and only TileGrid
            //If not then need to look up old tileID and new tileId. If oldTileID isn't in same texture then remove it and add it to new TileGrid.

            var oldTileID = TileDataGrid[x,y];
            var oldTextureIndex = Game._Game.TileTextureFactory.GetTextureTileIndexByTileID(oldTileID);
            var newTextureIndex = Game._Game.TileTextureFactory.GetTextureTileIndexByTileID(ID);

            if (oldTextureIndex != newTextureIndex)
            {

            }
            else
            {
                var tileGrid = TileGrids.FirstOrDefault(x => x.TileFactoryTextureID == oldTextureIndex);
                tileGrid.UpdateTile(x, y, ID);
            }

            TileDataGrid[x, y] = ID;
        }


        public static void CopyMultiDimensionalArray(Array sourceArray, Array destinationArray)
        {
            if (sourceArray.Rank != destinationArray.Rank)
            {
                throw new ArgumentException("Arrays must have the same number of dimensions.");
            }

            int[] lengths = new int[sourceArray.Rank];
            for (int i = 0; i < sourceArray.Rank; i++)
            {
                lengths[i] = sourceArray.GetLength(i);
            }

            CopyRecursive(sourceArray, destinationArray, new int[sourceArray.Rank], lengths, 0);
        }

        private static void CopyRecursive(Array sourceArray, Array destinationArray, int[] indices, int[] lengths, int dimension)
        {
            if (dimension == sourceArray.Rank)
            {
                destinationArray.SetValue(sourceArray.GetValue(indices), indices);
            }
            else
            {
                for (int i = 0; i < lengths[dimension]; i++)
                {
                    indices[dimension] = i;
                    CopyRecursive(sourceArray, destinationArray, indices, lengths, dimension + 1);
                }
            }
        }
    }
}
