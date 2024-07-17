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

                var convertedGridData = ConvertGridDataToTileData(gridData, null);

                TileGrids[0] = new TileGrid(convertedGridData, true, 0);
            }
            else
            {
                var texturesNeeded = Game._Game.TileTextureFactory.GetTextureIndicesForTileIDs(gridData);

                TileGrids = new TileGrid[texturesNeeded.Count];
                int i = 0;
                foreach (var textureID in texturesNeeded)
                {
                    var convertedGridData = ConvertGridDataToTileData(gridData, textureID);

                    TileGrids[i] = new TileGrid(convertedGridData, true, textureID);
                    TileGrids[i].Position.Z = i * .5f;
                    i++;
                }
            }

            TileDataGrid = gridData;
        }

        private TileGrid.TileData[,] ConvertGridDataToTileData(int[,] gridData, int? textureID)
        {
            var tileData = new TileGrid.TileData[gridData.GetLength(0), gridData.GetLength(1)];

            int index = 0;
            for (int w = 0; w < gridData.GetLength(0); w++)
            {
                for (int h = 0; h < gridData.GetLength(1); h++)
                {
                    tileData[w, h] = new TileGrid.TileData();

                    var textureTileID = Game._Game.TileTextureFactory.TileTextures[gridData[w, h]].TextureIndex;
                    if (textureID != null && textureTileID != textureID)
                    {
                        tileData[w, h].IsVisible = false;
                        tileData[w, h].TileID = 0;
                    }
                    else
                    {
                        tileData[w, h].IsVisible = true;
                        tileData[w, h].TileID = gridData[w, h];
                    }
                }
            }

            return tileData;
        }

        public void GPU_Use()
        {
            //TileGrids[0].GPU_Use();

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
