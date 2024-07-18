﻿using System;
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
        public TileGridLayer[] tileGridLayers;

        public TileGrid[] TileGrids = new TileGrid[5];


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

        /// <summary>
        /// Returns a 3d array that first index is layer number. index 0 = bottom layer
        /// </summary>
        public int[,,] VisibleTiles()
        {
            var ret = new int[CurrentLayer+1,3,3];

            int endIndex = 1;
            bool isVisible = false;
            for (int i = CurrentLayer; i >= 0; i--) //Does this layer have a transparent tile?
            {
                endIndex = i;

                for (int w = 0; w < 3; w++)
                {
                    for (int h = 0; h < 3; h++)
                    {
                        if (i == CurrentLayer)
                        {
                            ret[endIndex, w, h] = tileGridLayers[endIndex].TileDataGrid[w, h];
                        }
                        else //Check previous layer's tile and see if it is transparent
                        {
                            if (tileGridLayers[endIndex+1].TileDataGrid[w, h] == 8 || tileGridLayers[endIndex + 1].TileDataGrid[w, h] == 0)
                            {
                                ret[endIndex, w, h] = tileGridLayers[endIndex].TileDataGrid[w, h];
                            }
                            else
                            {
                                ret[endIndex, w, h] = -1;
                            }
                        }

                        if (tileGridLayers[endIndex].TileDataGrid[w, h] == 8 || tileGridLayers[endIndex].TileDataGrid[w, h] == 0)
                        {
                            if (endIndex != 0)
                            {
                                isVisible = true;
                            }
                        }
                    }
                }

                if (isVisible == false)
                    break;


            }

            //Remove layers by endIndex
            // Determine the size of the new array
            int newRowCount = ret.GetLength(0) - endIndex;
            int[,,] newArray = new int[newRowCount, 3,3];

            // Copy the elements
            for (int i = 0; i < newRowCount; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        newArray[i, j, k] = ret[endIndex + i, j, k];
                    }
                }
            }

            int index = 0;

            //Console.WriteLine($"Visible layers: {newArray.GetLength(0)}");

            //Console.WriteLine($"Layers Visible:");
            //for (int i = 0; i < newArray.GetLength(0); i++)
            //{
            //    for (int w = 0; w < 3; w++)
            //    {
            //        for (int h = 0; h < 3; h++)
            //        {
            //            Console.Write($"{newArray[i, w, h]}, ");
            //        }
            //        Console.Write($"{Environment.NewLine}");
            //    }
            //    Console.Write($"{Environment.NewLine}");
            //    index++;
            //}

            return newArray;
        }

        public void GPU_Use()
        {
            var tiles = VisibleTiles();

            for (int layer = 0; layer < tiles.GetLength(0); layer++)
            {
                var convertedGridData = ConvertGridDataToTileData(tiles, layer, null);

                if (TileGrids[layer] == null)
                {
                    TileGrids[layer] = new TileGrid(convertedGridData, true, 0);
                    var temp = layer + 1;
                    TileGrids[layer].Position.Z = temp * .15f;

                    if (IsTileGridAllAir(tiles, layer) == false)
                    {
                        TileGrids[layer].GPU_Use();
                    }
                }
                else
                {
                    TileGrids[layer].UpdateTile(convertedGridData);
                    TileGrids[layer].SendTiles();

                    if (IsTileGridAllAir(tiles, layer) == false)
                    {
                        TileGrids[layer].GPU_Use();
                    }
                }
            }

            //foreach (var tilegrid in TileGrids)
            //{
            //    if (tilegrid != null)
            //        tilegrid.GPU_Use();
            //}

            //int startIndex = 0;
            //for (int i = CurrentLayer; i >= 0; i--) //Does this layer have a transparent tile?
            //{
            //    startIndex = i;
            //    if (tileGridLayers[i].HasATransparentTile() == false)
            //        break;
            //}

            //for (int i = startIndex; i <= CurrentLayer; i++) //Does this layer have a transparent tile?
            //{
            //    tileGridLayers[i].GPU_Use();
            //}
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
