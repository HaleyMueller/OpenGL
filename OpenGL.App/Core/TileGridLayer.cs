using OpenGL.App.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenGL.App.Core.TileFactory.TileTextureFactory;
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

        public List<TileGrid> TileGrids { get; private set; } = new List<TileGrid>();

        public DateTime LastUsed { get; set; }

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
                TileGrids = new List<TileGrid>();

                var convertedGridData = ConvertGridDataToTileData(gridData, null);

                TileGrids.Add( new TileGrid(convertedGridData, true, 0));
            }
            else
            {
                var texturesNeeded = Game._Game.TileTextureFactory.GetTextureIndicesForTileIDs(gridData);

                TileGrids = new List<TileGrid>();
                int i = 0;
                foreach (var textureID in texturesNeeded)
                {
                    var convertedGridData = ConvertGridDataToTileData(gridData, textureID);

                    TileGrids.Add(new TileGrid(convertedGridData, true, textureID));
                    TileGrids[i].Position.Z = i * .15f;
                    i++;
                }
            }

            //TileDataGrid = gridData;
        }

        //internal bool HasATransparentTile()
        //{
        //    for (int w = 0; w < TileDataGrid.GetLength(0); w++)
        //    {
        //        for (int h = 0; h < TileDataGrid.GetLength(1); h++)
        //        {
        //            if (TileDataGrid[w, h] == 8)
        //            {
        //                return true;
        //            }
        //        }
        //    }

        //    return false;
        //}

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
                        //tileData[w, h].TileID = 0;
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

        public void LastUsedCheck()
        {
            for (int i = 0; i < TileGrids.Count; i++)
            {
                var gridLayer = TileGrids[i];
                if (gridLayer.LastUsed.AddSeconds(1) < DateTime.Now)
                {
                    Console.WriteLine($"Removing tile grid {i} from memory");
                    gridLayer.Dispose();
                    TileGrids.RemoveAt(i);
                }
            }
        }

        public void GPU_Use()
        {
            this.LastUsed = DateTime.Now;
            var gridData = TileDataGrid;
            //TileGrids[0].GPU_Use();
            var texturesNeeded = Game._Game.TileTextureFactory.GetTextureIndicesForTileIDs(gridData);
            int i = 1;
            foreach (var textureToUse in texturesNeeded)
            {
                var tileGrid = TileGrids.FirstOrDefault(x => x.TileFactoryTextureID == textureToUse);

                if (tileGrid != null)
                {
                    var temp = Layer + 1;
                    tileGrid.Position.Z = temp * .15f * i;

                    tileGrid.GPU_Use();

                    i++;
                }
                else
                {
                    Console.WriteLine("This shouldn't get hit rn");
                }
            }
        }

        public void SendTiles()
        {
            foreach (var grid in TileGrids)
            {
                grid.SendTiles();
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
                var tileGridOld = TileGrids.FirstOrDefault(x => x.TileFactoryTextureID == oldTextureIndex);
                tileGridOld.UpdateTile(x, y, false);

                var tileGridNew = TileGrids.FirstOrDefault(x => x.TileFactoryTextureID == newTextureIndex);

                if (tileGridNew == null)
                {
                    var texture = Game._Game.TileTextureFactory.TileTextures[ID];
                    tileGridNew = new TileGrid(3,3, true, texture.TextureIndex);
                    
                    TileGrids.Add(tileGridNew);
                    //tileGridNew.Position.Z = TileGrids.IndexOf(tileGridNew) + 1 * .15f;
                }
                tileGridNew.UpdateTile(x, y, ID);
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

        public void Dispose()
        {
            if (TileGrids != null)
            {
                for (int i = 0; i < TileGrids.Count; i++)
                {
                    TileGrids[i].Dispose();
                }
            }
        }
    }
}
