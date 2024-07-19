using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public List<TileGridLayer> TileGridLayers = new List<TileGridLayer>();

        public int[,,] ChunkData { get; set; }

        public int CurrentLayer { get; private set; }

        public int Width { get; private set; }
        public int Height { get; private set; }

        public TileGridView(int currentLayer, int[,,] data)
        {
            CurrentLayer = currentLayer;
            ChunkData = data;

            Width = data.GetLength(1);
            Height = data.GetLength(2);
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
        public ShaderTileData[,,] VisibleTiles()
        {
            //var ret = new int[CurrentLayer+1,Width,Height];
            var ret = new ShaderTileData[CurrentLayer+1,Width,Height];

            List<int> layerIsAllAir = new List<int>();

            bool[,] transparentBlocks = new bool[Width,Height];

            int endIndex = 1;
            bool isVisible = false;
            for (int i = CurrentLayer; i >= 0; i--) //Does this layer have a transparent tile?
            {
                endIndex = i;

                int airCount = 0;
                for (int w = 0; w < Width; w++)
                {
                    for (int h = 0; h < Height; h++)
                    {
                        ret[endIndex, w, h] = new ShaderTileData();
                        ret[endIndex, w, h].Depth = CurrentLayer - i;

                        if (ChunkData[i, w, h] == 0)
                        {
                            airCount++;
                        }

                        if (i == CurrentLayer)
                        {
                            ret[endIndex, w, h].TileID = ChunkData[i, w, h];

                            if (ret[endIndex , w, h].TileID == 8 || ret[endIndex, w, h].TileID == 0)
                            {
                                transparentBlocks[w, h] = true;
                            }
                            else
                            {
                                transparentBlocks[w, h] = false;
                            }

                            if (ret[endIndex, w, h].TileID == 0)
                            {
                                ret[endIndex, w, h].IsVisible = false;
                            }
                            else
                            {
                                ret[endIndex, w, h].IsVisible = true;
                            }
                        }
                        else //Check previous layer's tile and see if it is transparent
                        {
                            if (transparentBlocks[w, h]) //If under a transparent tile
                            {
                                ret[endIndex, w, h].TileID = ChunkData[i, w, h];

                                if (ret[endIndex, w, h].TileID == 8 || ret[endIndex, w, h].TileID == 0)
                                {
                                    transparentBlocks[w, h] = true;
                                }
                                else
                                {
                                    transparentBlocks[w, h] = false;
                                }

                                if (ret[endIndex, w, h].TileID == 0)
                                {
                                    ret[endIndex, w, h].IsVisible = false;
                                }
                                else
                                {
                                    ret[endIndex, w, h].IsVisible = true;
                                }
                            }
                            else
                            {
                                ret[endIndex, w, h].TileID = 0;
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

                if (airCount >= Width * Height)
                {
                    layerIsAllAir.Add(i);
                }

                if (isVisible == false)
                    break;
            }

            //Remove layers by endIndex
            // Determine the size of the new array
            int newRowCount = ret.GetLength(0) - endIndex - layerIsAllAir.Count;
            ShaderTileData[,,] newArray = new ShaderTileData[newRowCount, Width,Height];

            // Copy the elements
            int airOffset = 0;
            for (int i = 0; i < newRowCount; i++)
            {
                if (layerIsAllAir.Contains(i))
                {
                    airOffset++;
                }
                for (int j = 0; j < Width; j++)
                {
                    for (int k = 0; k < Height; k++)
                    {
                        newArray[i, j, k] = new ShaderTileData();
                        newArray[i, j, k].TileID = ret[endIndex + i + airOffset, j, k].TileID;
                        newArray[i, j, k].Depth = ret[endIndex + i + airOffset, j, k].Depth;
                    }
                }
            }

            //int index = 0;

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

        [DebuggerDisplay("TileID = {TileID} IsVisible = {IsVisible} Depth = {Depth}")]
        public class ShaderTileData
        {
            public int TileID { get; set; }
            public float Depth { get; set; }
            public bool IsVisible { get; set; }
        }

        public List<TileGridLayer.Dumb> AddOrUpdateTileGridLayer(int layer, ShaderTileData[,,] data)
        {
            var tileData = Convert3DimArrayTo2(layer, data);

            TileGridLayer tileGridLayer;
            if (layer >= TileGridLayers.Count)
            {
                tileGridLayer = new TileGridLayer(layer, tileData);
                TileGridLayers.Add(tileGridLayer);
                return TileGridLayers[layer].GPU_Use();
            }
            else if (TileGridLayers[layer] == null)
            {
                TileGridLayers[layer] = new TileGridLayer(layer, tileData);
                return TileGridLayers[layer].GPU_Use();
            }
            else
            {
                for (int i = 0; i < tileData.GetLength(0); i++)
                {
                    for (int x = 0; x < tileData.GetLength(1); x++)
                    {
                        TileGridLayers[layer].UpdateTileData(i, x, tileData[i, x]);
                    }
                }
                TileGridLayers[layer].SendTiles();
                return TileGridLayers[layer].GPU_Use();
            }
            
        }

        public void LastUsed()
        {
            for (int i = 0; i <  TileGridLayers.Count; i++)
            {
                var gridLayer = TileGridLayers[i];
                if (gridLayer.LastUsed.AddSeconds(1) < DateTime.Now)
                {
                    Console.WriteLine($"Removing tile grid layer {i} from memory");
                    gridLayer.Dispose();
                    TileGridLayers.RemoveAt(i);
                }
                else
                {
                    gridLayer.LastUsedCheck();
                }
            }
        }

        public class Hate
        {
            public int LayerID { get; set; }
            public List<TileGridLayer.Dumb> TileGridLayers { get; set; } = new List<TileGridLayer.Dumb>();
        }

        public List<Hate> GPU_Use()
        {
            var ret = new List<Hate>();
            var tiles = VisibleTiles();
            LastUsed();

            for (int layer = 0; layer < tiles.GetLength(0); layer++)
            {
                ret.Add(new Hate() { LayerID = layer, TileGridLayers = AddOrUpdateTileGridLayer(layer, tiles) });
            }

            return ret;
        }

        private ShaderTileData[,] Convert3DimArrayTo2(int index, ShaderTileData[,,] data)
        {
            var ret = new ShaderTileData[data.GetLength(1), data.GetLength(2)];

            for (var i = 0; i < ret.GetLength(0); i++)
            {
                for (var x = 0; x < ret.GetLength(1); x++)
                {
                    ret[i, x] = new ShaderTileData();
                    ret[i, x].TileID = data[index, i,x].TileID;
                    ret[i, x].Depth = data[index, i,x].Depth;
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

        //private TileGrid.TileData[,] ConvertGridDataToTileData(int[,,] gridData, int layerID, int? textureID)
        //{
        //    var tileData = new TileGrid.TileData[gridData.GetLength(1), gridData.GetLength(2)];

        //    int index = 0;
        //    for (int w = 0; w < gridData.GetLength(1); w++)
        //    {
        //        for (int h = 0; h < gridData.GetLength(2); h++)
        //        {
        //            tileData[w, h] = new TileGrid.TileData();

        //            var tileID = gridData[layerID, w, h];

        //            if (tileID < 0)
        //                tileID = 0;

        //            var textureTileID = Game._Game.TileTextureFactory.TileTextures[tileID].TextureIndex;
        //            if (textureID != null && textureTileID != textureID)
        //            {
        //                tileData[w, h].IsVisible = false;
        //                //tileData[w, h].TileID = 0;
        //            }
        //            else
        //            {
        //                tileData[w, h].IsVisible = true;
        //                tileData[w, h].TileID = tileID;
        //            }
        //        }
        //    }

        //    return tileData;
        //}
    }
}
