using OpenGL.App.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static OpenGL.App.Core.TileFactory.TileTextureFactory;
using static OpenGL.App.Core.TileGridView;
using static OpenGL.App.FFMPEG;

namespace OpenGL.App.Core
{
    /// <summary>
    /// Holds the data for the tile layer and instanced vbos
    /// </summary>
    public class TileGridLayer
    {
        public int Layer { get; private set; }
        public ShaderTileData[,] TileDataGrid { get; private set; }

        public List<TileGrid> TileGrids { get; private set; } = new List<TileGrid>();

        public DateTime LastUsed { get; set; }

        public int Width { get; private set; }
        public int Height { get; private set; }

        public TileGridLayer(int layer, ShaderTileData[,] tileDataGrid)
        {
            Layer = layer;
            TileDataGrid = tileDataGrid;

            Width = tileDataGrid.GetLength(0);
            Height = tileDataGrid.GetLength(1);

            SetTileGrids(tileDataGrid);
        }

        private void SetTileGrids(ShaderTileData[,] gridData)
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
        }

        private ShaderTileData[,] ConvertGridDataToTileData(ShaderTileData[,] gridData, int? textureID)
        {
            var tileData = new ShaderTileData[gridData.GetLength(0), gridData.GetLength(1)];

            int index = 0;
            for (int w = 0; w < gridData.GetLength(0); w++)
            {
                for (int h = 0; h < gridData.GetLength(1); h++)
                {
                    tileData[w, h] = new ShaderTileData();

                    var textureTileID = Game._Game.TileTextureFactory.TileTextures[gridData[w, h].TileID].TextureIndex;

                    tileData[w, h].Depth = gridData[w, h].Depth;

                    if (textureID != null && textureTileID != textureID)
                    {
                        tileData[w, h].IsVisible = false;
                        //tileData[w, h].TileID = 0;
                    }
                    else
                    {
                        tileData[w, h].IsVisible = true;
                        tileData[w, h].TileID = gridData[w, h].TileID;
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

        public class Dumb
        {
            public int TileFactoryTextureID { get; set; }
            public float Position { get; set; }
        }

        public List<Dumb> GPU_Use()
        {
            var ret = new List<Dumb>();

            this.LastUsed = DateTime.Now;
            var gridData = TileDataGrid;
            //TileGrids[0].GPU_Use();
            var texturesNeeded = Game._Game.TileTextureFactory.GetTextureIndicesForTileIDs(gridData);
            float i = .01f;
            foreach (var textureToUse in texturesNeeded)
            {
                var tileGrid = TileGrids.FirstOrDefault(x => x.TileFactoryTextureID == textureToUse);

                if (tileGrid != null)
                {
                    var temp = (Layer * 2);
                    tileGrid.Position.Z = temp + i;

                    tileGrid.GPU_Use();

                    ret.Add(new Dumb() { TileFactoryTextureID = textureToUse, Position = tileGrid.Position.Z });

                    i++;
                }
                else
                {
                    Console.WriteLine("This shouldn't get hit rn");
                }
            }

            return ret;
        }

        public void SendTiles()
        {
            foreach (var grid in TileGrids)
            {
                grid.SendTiles();
            }
        }

        public void UpdateTileData(int x, int y, ShaderTileData ID)
        {
            //If bindless then use first and only TileGrid
            //If not then need to look up old tileID and new tileId. If oldTileID isn't in same texture then remove it and add it to new TileGrid.

            var oldTileID = TileDataGrid[x,y];
            var oldTextureIndex = Game._Game.TileTextureFactory.GetTextureTileIndexByTileID(oldTileID.TileID);
            var newTextureIndex = Game._Game.TileTextureFactory.GetTextureTileIndexByTileID(ID.TileID);

            if (oldTextureIndex != newTextureIndex)
            {
                var tileGridOld = TileGrids.FirstOrDefault(x => x.TileFactoryTextureID == oldTextureIndex);
                tileGridOld.HideTile(x, y);

                var tileGridNew = TileGrids.FirstOrDefault(x => x.TileFactoryTextureID == newTextureIndex);

                if (tileGridNew == null)
                {
                    var texture = Game._Game.TileTextureFactory.TileTextures[ID.TileID];
                    tileGridNew = new TileGrid(Width,Height, true, texture.TextureIndex);
                    
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

            TileDataGrid[x, y].TileID = ID.TileID;
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
