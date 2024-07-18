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
                            if (tileGridLayers[endIndex+1].TileDataGrid[w, h] == 8)
                            {
                                ret[endIndex, w, h] = tileGridLayers[endIndex].TileDataGrid[w, h];
                            }
                            else
                            {
                                ret[endIndex, w, h] = -1;
                            }
                        }

                        if (tileGridLayers[endIndex].TileDataGrid[w, h] == 8)
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

                if (tileGridLayers[i].HasATransparentTile() == false)
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
            VisibleTiles();
            int startIndex = 0;
            for (int i = CurrentLayer; i >= 0; i--) //Does this layer have a transparent tile?
            {
                startIndex = i;
                if (tileGridLayers[i].HasATransparentTile() == false)
                    break;
            }

            for (int i = startIndex; i <= CurrentLayer; i++) //Does this layer have a transparent tile?
            {
                tileGridLayers[i].GPU_Use();
            }
        }
    }
}
