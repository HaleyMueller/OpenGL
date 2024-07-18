using OpenGL.App.Core.Shader;
using OpenGL.App.Core.Texture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace OpenGL.App.Core
{
    public class TileFactory
    {
        public Tile[] Tiles { get; set; }
        public TileFactory() 
        {
            Tiles = new Tile[9]; //TODO make this create from json by name and then map it to int id
            Tiles[0] = new Tile() { Name = "air"};
            Tiles[1] = new Tile() { Name = "dirt"};
            Tiles[2] = new Tile() { Name = "sand"};
            Tiles[3] = new Tile() { Name = "stone"};
            Tiles[4] = new Tile() { Name = "spruce_leaves"};
            Tiles[5] = new Tile() { Name = "quartz_block_top" };
            Tiles[6] = new Tile() { Name = "coal_block" };
            Tiles[7] = new Tile() { Name = "deepslate" };
            Tiles[8] = new Tile() { Name = "glass" };

            int i = 0;
            Console.WriteLine("Registering tiles");
            foreach (var tile in Tiles)
            {
                Console.WriteLine($"[{i}] {tile.Name}");
                i++;
            }
        }

        public const int TileResolution = 16;

        public class TileTextureFactory
        {
            public TileFactory TileFactory { get; set; }
            public List<Texture.Texture> Textures { get; set; } = new List<Texture.Texture>(); //Need to bind what texture and index they are for tile type
            public TileTexture[] TileTextures { get; set; } //Index is based on tile id

            public class TileTexture //Class that references what GPU Texture to use
            {
                public int TextureIndex { get; set; }
                public int TextureDepth { get; set; }
            }

            public TileTextureFactory(TileFactory tileFactory)
            {
                TileFactory = tileFactory;

                TileTextures = new TileTexture[TileFactory.Tiles.Length];

                if (Game._Game.IsBindlessSupported) //Create bindless textures
                {
                    BindlessTexture bt = new BindlessTexture("Resources/Textures");

                    Textures.Add(bt);

                    int textureDepth = 0;
                    foreach (var fileName in bt.TextureFiles)
                    {
                        var tileTexture = new TileTexture();
                        tileTexture.TextureIndex = 0; //Should only ever have 1 bindless texture since it is infinite
                        tileTexture.TextureDepth = textureDepth;

                        FileInfo fileInfo = new FileInfo(fileName);
                        string fileNameWithoutExtenstion = fileInfo.Name.Replace(fileInfo.Extension, string.Empty);
                        var tileID = TileFactory.Tiles.ToList().IndexOf(TileFactory.Tiles.FirstOrDefault(x => x.Name == fileNameWithoutExtenstion)); //Convert file name to index of TileFactory.Tiles

                        if (tileID == -1)
                            Console.WriteLine($"Couldn't find tile object with name of: {fileNameWithoutExtenstion}");
                        else
                            TileTextures[tileID] = tileTexture;

                        textureDepth++;
                    }
                }
                else //Create texture arrays
                {
                    if (TileResolution % 2 != 0)
                    {
                        throw new Exception("Tile Resolution has to be a multiple of 2.");
                    }

                    var textureArraysNeeded = Math.Round((decimal)TileFactory.Tiles.Length / Game._Game.MaxArrayTextureLayers, MidpointRounding.ToPositiveInfinity); //How many texture arrays do we need?

                    Console.WriteLine($"Texture Arrays needed: {textureArraysNeeded}");

                    int fileOffset = 0;
                    for (int i = 0; i < textureArraysNeeded; i++)
                    {
                        var textureArray = new TextureArray("Resources/Textures", Game._Game.MaxArrayTextureLayers, fileOffset, resolution: TileResolution);
                        fileOffset += textureArray.TextureFiles.Length;
                        Textures.Add(textureArray);

                        int textureDepth = 0;
                        foreach (var fileName in textureArray.TextureFiles)
                        {
                            var tileTexture = new TileTexture();
                            tileTexture.TextureIndex = i;
                            tileTexture.TextureDepth = textureDepth;

                            FileInfo fileInfo = new FileInfo(fileName);
                            string fileNameWithoutExtenstion = fileInfo.Name.Replace(fileInfo.Extension, string.Empty);
                            var tileID = TileFactory.Tiles.ToList().IndexOf(TileFactory.Tiles.FirstOrDefault(x => x.Name == fileNameWithoutExtenstion)); //Convert file name to index of TileFactory.Tiles

                            if (tileID == -1)
                                Console.WriteLine($"Couldn't find tile object with name of: {fileNameWithoutExtenstion}");
                            else
                                TileTextures[tileID] = tileTexture;

                            Console.WriteLine($"{fileName} is in texture array {i} at depth {textureDepth}");

                            textureDepth++;
                        }
                    }
                }
            }

            /// <summary>
            /// Returns the texture tile index in relation to game tile id
            /// </summary>
            public int GetTextureTileIDByTileID(int tileID)
            {
                //Find what texture to use and what tileid
                var tileTexture = TileTextures[tileID];

                return tileTexture.TextureDepth;
            }

            public List<int> GetTextureIndicesForTileIDs(int[,] tileIDs)
            {
                var ret = new List<int>();

                for (int x = 0; x < tileIDs.GetLength(0); x++)
                {
                    for (int y = 0; y < tileIDs.GetLength(1); y++)
                    {
                        var tileTexture = TileTextures[tileIDs[x, y]]; //Grab tileID
                        ret.Add(tileTexture.TextureIndex);
                    }
                }

                ret = ret.Distinct().ToList();

                return ret;
            }

            public int GetTextureTileIndexByTileID(int tileID)
            {
                var tileTexture = TileTextures[tileID]; //Grab tileID
                return tileTexture.TextureIndex;
            }

            public void GPU_UseByTextureID(int TileFactoryTextureID, ShaderProgram shaderProgram, Texture.Texture.TextureData textureData)
            {
                //Find what texture to use and what tileid
                var texture = Textures[TileFactoryTextureID];

                texture.GPU_Use(textureData);
            }

            public void GPU_Use(int tileID, ShaderProgram shaderProgram, Texture.Texture.TextureData textureData, bool isInstanced = false)
            {
                //Find what texture to use and what tileid
                var tileTexture = TileTextures[tileID];
                var texture = Textures[tileTexture.TextureIndex];

                if (isInstanced == false)
                {
                    if (Game._Game.IsBindlessSupported == false)
                        shaderProgram.SetUniform("selectedTexture", tileTexture.TextureDepth);
                    else
                        textureData.SelectedTexture = tileTexture.TextureDepth;
                }

                texture.GPU_Use(textureData);
            }
        }

        public class Tile
        {
            public string Name { get; set; }
        }
    }
}
