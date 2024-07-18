using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace OpenGL.App.Core
{
    /// <summary>
    /// Top level class. This is where the data for the tile is going to be used for telling what tiles to show on screen using the current camera layer
    /// </summary>
    public class TileGridView
    {
        public TileGrid[] TileGrids = new TileGrid[5];
        public TileGridLayer[] TileGridLayers = new TileGridLayer[5];

        public int[,,] ChunkData { get; set; }

        public int CurrentLayer { get; private set; }

        public TileGridView(int currentLayer, int[,,] data)
        {
            CurrentLayer = currentLayer;
            ChunkData = data;
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

            if (CurrentLayer >= ChunkData.GetLength(0))
            {
                CurrentLayer = ChunkData.GetLength(0) - 1;
            }
        }

        /// <summary>
        /// Returns a 3d array that first index is layer number. index 0 = bottom layer
        /// </summary>
        public int[,,] VisibleTiles()
        {
            var ret = new int[CurrentLayer+1,3,3];

            List<int> layerIsAllAir = new List<int>();

            int endIndex = 1;
            bool isVisible = false;
            for (int i = CurrentLayer; i >= 0; i--) //Does this layer have a transparent tile?
            {
                endIndex = i;

                int airCount = 0;
                for (int w = 0; w < 3; w++)
                {
                    for (int h = 0; h < 3; h++)
                    {
                        if (ChunkData[i, w, h] == 0)
                        {
                            airCount++;
                        }

                        if (i == CurrentLayer)
                        {
                            ret[endIndex, w, h] = ChunkData[i, w, h];
                        }
                        else //Check previous layer's tile and see if it is transparent
                        {
                            if (ret[endIndex+1,w, h] == 8 || ret[endIndex + 1,w, h] == 0)
                            {
                                ret[endIndex, w, h] = ChunkData[i, w, h];
                            }
                            else
                            {
                                ret[endIndex, w, h] = 0;
                            }
                        }

                        if (ChunkData[i, w, h] == 8 || ChunkData[i, w, h] == 0)
                        {
                            if (endIndex != 0)
                            {
                                isVisible = true;
                            }
                        }
                    }
                }

                if (airCount >= 3 * 3)
                {
                    layerIsAllAir.Add(i);
                }

                if (isVisible == false)
                    break;
            }

            //Remove layers by endIndex
            // Determine the size of the new array
            int newRowCount = ret.GetLength(0) - endIndex - layerIsAllAir.Count;
            int[,,] newArray = new int[newRowCount, 3,3];

            // Copy the elements
            int airOffset = 0;
            for (int i = 0; i < newRowCount; i++)
            {
                if (layerIsAllAir.Contains(i))
                {
                    airOffset++;
                }
                for (int j = 0; j < 3; j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        newArray[i, j, k] = ret[endIndex + i + airOffset, j, k];
                    }
                }
            }

            int index = 0;

            Console.WriteLine($"Visible layers: {newArray.GetLength(0)}");

            Console.WriteLine($"Layers Visible:");
            for (int i = 0; i < newArray.GetLength(0); i++)
            {
                for (int w = 0; w < 3; w++)
                {
                    for (int h = 0; h < 3; h++)
                    {
                        Console.Write($"{newArray[i, w, h]}, ");
                    }
                    Console.Write($"{Environment.NewLine}");
                }
                Console.Write($"{Environment.NewLine}");
                index++;
            }

            return newArray;
        }

        public void GPU_Use()
        {
            var tiles = VisibleTiles();

            for (int layer = 0; layer < tiles.GetLength(0); layer++)
            {
                var convertedGridData = ConvertGridDataToTileData(tiles, layer, null);

                if (TileGridLayers[layer] == null)
                {
                    TileGridLayers[layer] = new TileGridLayer(layer, Convert3DimArrayTo2(layer, tiles));
                    var temp = layer + 1;

                    foreach (var grid in TileGridLayers[layer].TileGrids)
                    {
                        grid.Position.Z = temp * .15f;
                    }
                    TileGridLayers[layer].GPU_Use();
                }
                else
                {
                    for (int i = 0; i < tiles.GetLength(1); i++)
                    {
                        for (int x = 0; x < tiles.GetLength(2); x++)
                        {
                            TileGridLayers[layer].UpdateTileData(i, x, tiles[layer,i,x]);
                        }
                    }
                    
                    TileGridLayers[layer].SendTiles();
                    TileGridLayers[layer].GPU_Use();
                }
            }
        }

        private int[,] Convert3DimArrayTo2(int index, int[,,] data)
        {
            var ret = new int[data.GetLength(1), data.GetLength(2)];

            for (var i = 0; i < ret.GetLength(0); i++)
            {
                for (var x = 0; x < ret.GetLength(1); x++)
                {
                    ret[i,x] = data[index, i,x];
                }
            }

            return ret;
        }

        private bool IsTileGridAllAir(int[,,] gridData, int layer)
        {
            var ret = false;

            var airCount = 0;
            for (int i = 0; i < gridData.GetLength(1); i++)
            {
                for (int x = 0; x < gridData.GetLength(2); x++)
                {
                    if (gridData[layer,i,x] != 0)
                    {
                        i = gridData.GetLength(1);
                        x = gridData.GetLength(2);
                        airCount++;
                    }
                }
            }

            if (airCount >= gridData.GetLength(1) * gridData.GetLength(2))
                return true;

            return ret;
        }

        private TileGrid.TileData[,] ConvertGridDataToTileData(int[,,] gridData, int layerID, int? textureID)
        {
            var tileData = new TileGrid.TileData[gridData.GetLength(1), gridData.GetLength(2)];

            int index = 0;
            for (int w = 0; w < gridData.GetLength(1); w++)
            {
                for (int h = 0; h < gridData.GetLength(2); h++)
                {
                    tileData[w, h] = new TileGrid.TileData();

                    var tileID = gridData[layerID, w, h];

                    if (tileID < 0)
                        tileID = 0;

                    var textureTileID = Game._Game.TileTextureFactory.TileTextures[tileID].TextureIndex;
                    if (textureID != null && textureTileID != textureID)
                    {
                        tileData[w, h].IsVisible = false;
                        //tileData[w, h].TileID = 0;
                    }
                    else
                    {
                        tileData[w, h].IsVisible = true;
                        tileData[w, h].TileID = tileID;
                    }
                }
            }

            return tileData;
        }
    }
}
