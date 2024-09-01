using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Graphics;
using System.Drawing.Imaging;
using System.Drawing;
using OpenGL.App.Core.Shader;
using OpenGL.App.Core.Texture;
using OpenGL.App.Core.Vertex;
using FmodAudio;
using FmodAudio.DigitalSignalProcessing;
using System.Diagnostics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenGL.App.Core;
using OpenGL.App.Core.UniformBufferObject;
using System.Security.Cryptography.X509Certificates;
using OpenGL.App.GameObjects;
using static OpenGL.App.FFMPEG;
using OpenGL.App.Core.SSBO;
using static OpenGL.App.Core.SSBO.SSBOFactory;
using static OpenGL.App.Core.Texture.BindlessTexture;
using Icaria.Engine.Procedural;
using static OpenCvSharp.ML.DTrees;

namespace OpenGL.App
{
    public class Game : GameWindow
    {
        public bool PlayVideo = true;
        public bool WorldGen = true;

        public static Game _Game;

        public Camera MainCamera;

        public UniformBufferObjectFactory UBOFactory { get; private set; }
        public SSBOFactory SSBOFactory { get; private set; }
        public ShaderFactory ShaderFactory { get; private set; }
        public TileFactory TileFactory { get; private set; }
        public TileFactory.TileTextureFactory TileTextureFactory { get; private set; }
        private FreeTypeFont _font;

        public TextureArray TextureArray;
        public BindlessTexture BindlessTexture;

        public TileGameObject Tile;

        public Game(int width = 1280, int height = 768) : base(
            GameWindowSettings.Default,
            new NativeWindowSettings()
            {
                Title = "Title",
                Size = new Vector2i(width, height),
                WindowBorder = WindowBorder.Fixed,
                StartVisible = true,
                StartFocused = true,
                API = ContextAPI.OpenGL,
                Profile = ContextProfile.Core,
                APIVersion = new Version(3, 3)
            })
        {
            this.CenterWindow();
            _Game = this;
            this.MouseMove += Game_MouseMove;
        }

        private void Game_MouseMove(MouseMoveEventArgs obj)
        {
            if (this.CursorState == CursorState.Grabbed)
            {
                //MainCamera.ProcessMouseMovement(obj.DeltaX, obj.DeltaY);
            }
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, e.Width, e.Height);
            base.OnResize(e);
        }

        bool rainbowImage = false;

        FmodAudio.FmodSystem fmodSystem;
        FmodAudio.Channel fmodChannel;
        FmodAudio.Sound sound;

        public bool IsBindlessSupported;
        public int MaxArrayTextureLayers;


        public Video VideoFromFile;

        public TileGridLayer TileGridLayer;
        public TileGridView TileGridView;

        protected override async void OnLoad()
        {
            IsBindlessSupported = IsBindlessTextureSupported();
            MaxArrayTextureLayers = GL.GetInteger(GetPName.MaxArrayTextureLayers);

            #if DEBUG
            //IsBindlessSupported = false;
            MaxArrayTextureLayers = 3;
            #endif

            this.IsVisible = true;

            GL.ClearColor(Color4.DarkCyan); //Set up clear color

            UBOFactory = new UniformBufferObjectFactory();
            SSBOFactory = new SSBOFactory();
            ShaderFactory = new ShaderFactory();
            TileFactory = new TileFactory();
            TileTextureFactory = new TileFactory.TileTextureFactory(TileFactory);

            MainCamera = new Camera(new Vector3(0, 0, 10f), null);

            _font = new FreeTypeFont();

            Stopwatch.Start();

            if (PlayVideo)
            {
                fmodSystem = FmodAudio.Fmod.CreateSystem();
                fmodSystem.Init(32, FmodAudio.InitFlags.Normal);
                sound = fmodSystem.CreateSound("Resources/Sounds/badapple.mp3");

                Console.WriteLine("Loading video file...");

                FFMPEG fFMPEG = new FFMPEG();
                VideoFromFile = await fFMPEG.MakeVideoIntoBitmaps("Resources/Videos/badapple.mp4", "tmp");

                //VideoFromFile = Program.ExtractFrames("Resources/Videos/badapple.mp4", .25f);
                System.Diagnostics.Debugger.Break();
                
                var x = VideoFromFile.Width / 2;
                var y = VideoFromFile.Height / 2;

                //MainCamera = new Camera(new Vector3(80, 65, 3), null, zoom: 17);
                MainCamera = new Camera(new Vector3(x, y, 3), null, zoom: 5);

                int[,,] tileData = new int[1, VideoFromFile.Width, VideoFromFile.Height];

                for (int i = 0; i < tileData.GetLength(1); i++)
                {
                    for (int j = 0; j < tileData.GetLength(2); j++)
                    {
                        int tileID = 6;
                        if (VideoFromFile.Frames.FirstOrDefault().Pixels[i, j])
                        {
                            tileID = 5;
                        }

                        tileData[0, i, j] = tileID;
                    }
                }

                TileGridView = new TileGridView(0, tileData);
            }
            else if (WorldGen)
            {
                int width = 100;
                int height = 100;
                int depth = 100;

                int seed = 6942069;
                float[,,] heightMap = GenerateHeightMap(seed, width, height, depth);

                int[,,] tileData = new int[depth, width, height];

                for (int d = 0; d < heightMap.GetLength(0); d++)
                {
                    int[,] tileGridLayerData = new int[heightMap.GetLength(1), heightMap.GetLength(2)];

                    for (int x = 0; x < heightMap.GetLength(1); x++)
                    {
                        for (int y = 0; y < heightMap.GetLength(2); y++)
                        {
                            var block = heightMap[d, x, y];

                            if (block <= .1f)
                                tileGridLayerData[x, y] = 1;
                            else if (block <= .2f)
                                tileGridLayerData[x, y] = 2;
                            else if (block <= .3f)
                                tileGridLayerData[x, y] = 3;
                            else if (block <= .4f)
                                tileGridLayerData[x, y] = 4;
                            else if (block <= .5f)
                                tileGridLayerData[x, y] = 5;
                            else if (block <= .6f)
                                tileGridLayerData[x, y] = 6;
                            else if (block <= .7f)
                                tileGridLayerData[x, y] = 7;
                            else if (block <= .8f)
                                tileGridLayerData[x, y] = 8;
                            else
                                tileGridLayerData[x, y] = 9;
                        }
                    }

                    UpdateTileChunkData(tileGridLayerData, d, ref tileData);
                }

                TileGridView = new TileGridView(2, tileData);
            }
            else
            {
                int[,,] tileData = new int[5,3,4];

                int[,] tileGridLayerData =
                {
                    { 1, 1, 1, 1 },
                    { 1, 1, 1, 1 },
                    { 1, 1, 1, 1 }
                };

                UpdateTileChunkData(tileGridLayerData, 4, ref tileData);

                tileGridLayerData = new int[,]
{
                    { 1, 2, 3, 2 },
                    { 3, 8, 1, 2 },
                    { 1, 1, 1, 2 }
                };

                UpdateTileChunkData(tileGridLayerData, 3, ref tileData);

                tileGridLayerData = new int[,]
{
                    { 0, 0, 0, 0 },
                    { 0, 0, 0, 0 },
                    { 0, 0, 0, 0 }
                };

                UpdateTileChunkData(tileGridLayerData, 2, ref tileData);

                tileGridLayerData = new int[,]
                {
                    { 7, 7, 7, 4 },
                    { 7, 8, 7, 4 },
                    { 7, 7, 7, 4 }
                };

                UpdateTileChunkData(tileGridLayerData, 1, ref tileData);

                tileGridLayerData = new int[,]
{
                    { 9, 9, 9, 5 },
                    { 9, 9, 9, 5 },
                    { 9, 9, 9, 5 }
                };

                UpdateTileChunkData(tileGridLayerData, 0, ref tileData);

                TileGridView = new TileGridView(2, tileData);
            }

            Tile = new TileGameObject(0, 0);
            Tile.SetTileID(0);

            GL.Enable(EnableCap.DepthTest);

            base.OnLoad();
        }

        float[,,] GenerateHeightMap(int seed, int width, int height, int depth)
        {
            float[,,] heightMap = new float[depth, width, height];

            for (int d = 0; d < depth; d++)
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        //Console.WriteLine($"ret {IcariaNoise.GradientNoise3D(x, y, d, seed)}");
                        //heightMap[d, x, y] = IcariaNoise.GradientNoise3D(x, y, d, seed);
                        heightMap[d, x, y] = IcariaNoise.GradientNoiseHQ(x, y, seed);
                    }
                }
            }
            return heightMap;
        }

        private void UpdateTileChunkData(int[,] data, int layer, ref int[,,] chunk)
        {
            for (int i = 0; i < data.GetLength(0); i++)
            {
                for (int j = 0; j < data.GetLength(1); j++)
                {
                    chunk[layer, i, j] = data[i, j];
                }
            }
        }

        private bool IsBindlessTextureSupported()
        {
            int numberOfExtensions = GL.GetInteger(GetPName.NumExtensions);
            for (int i = 0; i < numberOfExtensions; i++)
            {
                string extension = GL.GetString(StringNameIndexed.Extensions, i);
                if (extension == "GL_ARB_bindless_texture")
                {
                    return true;
                }
            }
            return false;
        }

        protected override void OnUnload()
        {
            //Removing everything
            ShaderFactory.Dispose();

            if (PlayVideo)
                fmodSystem.Release();

            base.OnUnload();
        }

        int realFrameNumber = 0;
        double frameNumber = 0;
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            if (PlayVideo)
            {
                if (frameNumber == 0)
                {
                    fmodSystem.PlaySound(sound);
                }

                fmodSystem.Update();
                frameNumber = (frameNumber + (VideoFromFile.FPS * args.Time));

                if (realFrameNumber != (int)frameNumber) //Brand new frame. Update the vbo
                {
                    realFrameNumber = (int)frameNumber;

                    if (realFrameNumber >= VideoFromFile.Frames.Length)
                    {
                        PlayVideo = false;
                    }
                    else
                    {
                        Console.WriteLine($"Frame: {frameNumber}");

                        var frame = VideoFromFile.Frames[(int)frameNumber];

                        //TileGridView.ShaderTileData[,,] tileData = new TileGridView.ShaderTileData[1, VideoFromFile.Width, VideoFromFile.Height];

                        int[,,] tileData = new int[1, VideoFromFile.Width, VideoFromFile.Height];
                        TileGridView.ShaderTileData[,,] tileDataa = new TileGridView.ShaderTileData[1, VideoFromFile.Width, VideoFromFile.Height];

                        int[,] tileGridLayerData = new int[VideoFromFile.Width, VideoFromFile.Height];

                        for (int w = 0; w < VideoFromFile.Width; w++)
                        {
                            for (int h = 0; h < VideoFromFile.Height; h++)
                            {
                                var pixel = frame.Pixels[w, h];

                                var width = (VideoFromFile.Width - w) - 1;
                                var height = (VideoFromFile.Height - h) - 1;

                                int tileID = 6;
                                if (frame.Pixels[w, h])
                                {
                                    tileID = 5;
                                }

                                tileGridLayerData[w, h] = tileID;

                                tileDataa[0, w, h] = new TileGridView.ShaderTileData() { Depth = 0, IsVisible = true, TileID = tileID };


                                //TileGrid.UpdateTile(w, height, (pixel ? 6 : 4));
                            }
                        }

                        //UpdateTileChunkData(tileGridLayerData, 0, ref TileGridView.ChunkData);
                        TileGridView.SendTiles(0, tileDataa, false);

                        //TileGrid.SendTiles();
                    }
                }
            }

            if (_Game.IsKeyPressed(Keys.Comma))
                TileGridView.DecreaseLayer();
            if (_Game.IsKeyPressed(Keys.Period))
                TileGridView.IncreaseLayer();

            #region Camera
            float cameraSpeed = (float)(1f * args.Time);

            if (_Game.IsKeyPressed(Keys.Escape))
                this.CursorState = this.CursorState == CursorState.Grabbed ? CursorState.Normal : CursorState.Grabbed;

            if (_Game.IsKeyDown(Keys.W))
                MainCamera.ProcessKeyboard(Camera.Camera_Movement.FORWARD, (float)args.Time);
            else if (_Game.IsKeyDown(Keys.S))
                MainCamera.ProcessKeyboard(Camera.Camera_Movement.BACKWARD, (float)args.Time);

            if (_Game.IsKeyDown(Keys.A))
                MainCamera.ProcessKeyboard(Camera.Camera_Movement.LEFT, (float)args.Time);
            else if (_Game.IsKeyDown(Keys.D))
                MainCamera.ProcessKeyboard(Camera.Camera_Movement.RIGHT, (float)args.Time);

            if (_Game.MouseState.ScrollDelta != Vector2.Zero)
                MainCamera.ProcessMouseScroll(_Game.MouseState.ScrollDelta.Y);
            #endregion

            base.OnUpdateFrame(args);
        }

        List<long> timeSpans = new List<long>();
        TimeSpan limit = new TimeSpan(0, 0, 5);
        int framesDuringLimit = 0;

        Stopwatch Stopwatch = new Stopwatch();

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            #region FPS
            timeSpans.Add(DateTime.Now.Ticks);

            for (int i = timeSpans.Count - 1; i >= 0; i--)
            {
                long tick = timeSpans[i];

                if (TimeSpan.FromTicks(tick).Add(limit).Ticks <= DateTime.Now.Ticks)
                {
                    timeSpans.Remove(timeSpans.ElementAt(i));
                }
            }

            framesDuringLimit = (int)(timeSpans.Count() / limit.TotalSeconds);
            #endregion

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit); //Clear color buffer

            //Load up and set data for the Projection/View Matrix ubo
            UniformBufferObjectFactory.UniformBufferObjects[UniformBufferObjectFactory.UBOIndex.ProjectionViewMatrix].GPU_Use();

            //Draw Objects
            var displayData = TileGridView.GPU_Use();
            //TileGrid.GPU_Use();
            Tile.GPU_Use();

            GL.Clear(ClearBufferMask.DepthBufferBit); //Clear depth buffer for ui to be on top

            GL.UseProgram(ShaderFactory.ShaderPrograms["TextShader.glsl"].ShaderProgramHandle);
            _font.RenderText($"FPS: {framesDuringLimit}", new Vector2(.5f, 12f), .25f, new Color4(1f, .8f, 1f, 1f));
            _font.RenderText($"IsBindlessSupported: {IsBindlessSupported}", new Vector2(.5f, 25f), .25f, new Color4(1f, .8f, 1f, 1f));
            _font.RenderText($"Layer: {TileGridView.CurrentLayer}", new Vector2(.5f, 37f), .25f, new Color4(1f, .8f, 1f, 1f));

            float height = 37f;
            foreach (var layer in displayData)
            {
                height += 13f;
                _font.RenderText($"Layer ID: {layer.LayerID}", new Vector2(.5f, height), .25f, new Color4(1f, .8f, 1f, 1f));
                foreach (var tileGrid in layer.TileGridLayers)
                {
                    height += 13f;
                    _font.RenderText($"Texture ID: {tileGrid.TileFactoryTextureID}", new Vector2(20f, height), .25f, new Color4(1f, .8f, 1f, 1f));
                    height += 13f;
                    _font.RenderText($"X Pos: {tileGrid.Position}", new Vector2(20f, height), .25f, new Color4(1f, .8f, 1f, 1f));
                }
            }

            this.Context.SwapBuffers(); //Take back buffer into forground buffer

            base.OnRenderFrame(args);
        }
    }
}
