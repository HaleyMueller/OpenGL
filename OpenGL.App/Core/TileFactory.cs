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
            Tiles = new Tile[4]; //TODO make this create from json by name and then map it to int id
            Tiles[0] = new Tile() { Name = "dirt"};
            Tiles[1] = new Tile() { Name = "sand"};
            Tiles[2] = new Tile() { Name = "stone"};
            Tiles[3] = new Tile() { Name = "spruce_leaves"};
        }

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
                    var textureArraysNeeded = Math.Round((decimal)TileFactory.Tiles.Length / Game._Game.MaxArrayTextureLayers, MidpointRounding.ToPositiveInfinity); //How many texture arrays do we need?

                    int fileOffset = 0;
                    for (int i = 0; i < textureArraysNeeded; i++)
                    {
                        var textureArray = new TextureArray("Resources/Textures", Game._Game.MaxArrayTextureLayers, fileOffset);
                        fileOffset += textureArray.TextureFiles.Length;
                        Textures.Add(textureArray);

                        int textureDepth = 0;
                        foreach (var fileName in textureArray.TextureFiles)
                        {
                            var tileTexture = new TileTexture();
                            tileTexture.TextureIndex = i; //Should only ever have 1 bindless texture since it is infinite
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
                }
            }

            public void GPU_Use(int tileID, ShaderProgram shaderProgram, Texture.Texture.TextureData textureData)
            {
                //Find what texture to use and what tileid
                var tileTexture = TileTextures[tileID];
                var texture = Textures[tileTexture.TextureIndex];

                if (Game._Game.IsBindlessSupported == false)
                    shaderProgram.SetUniform("selectedTexture", tileTexture.TextureDepth);
                else
                    textureData.SelectedTexture = tileTexture.TextureDepth;

                texture.GPU_Use(textureData);
            }
        }

        public class Tile
        {
            public string Name { get; set; }
        }
    }
}
