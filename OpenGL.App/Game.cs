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

namespace OpenGL.App
{
    public class Game : GameWindow
    {
        public static Game _Game;

        public Camera MainCamera;

        public UniformBufferObjectFactory UBOFactory { get; private set; }
        public ShaderFactory ShaderFactory { get; private set; }
        public TileFactory TileFactory { get; private set; }
        public TileFactory.TileTextureFactory TileTextureFactory { get; private set; }
        private FreeTypeFont _font;

        public TextureArray TextureArray;
        public BindlessTexture BindlessTexture;

        public TileGrid TileGrid;
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

        //public TileGameObject Tile;

        protected override void OnLoad()
        {
            IsBindlessSupported = IsBindlessTextureSupported();
            MaxArrayTextureLayers = GL.GetInteger(GetPName.MaxArrayTextureLayers);

            #if DEBUG
            IsBindlessSupported = true;
            //MaxArrayTextureLayers = 2;
            #endif

            this.IsVisible = true;

            GL.ClearColor(Color4.DarkCyan); //Set up clear color

            UBOFactory = new UniformBufferObjectFactory();
            ShaderFactory = new ShaderFactory();
            TileFactory = new TileFactory();
            TileTextureFactory = new TileFactory.TileTextureFactory(TileFactory);

            MainCamera = new Camera(new Vector3(0, 0, 3f), null);

            _font = new FreeTypeFont();

            Stopwatch.Start();

            TileGrid = new TileGrid(100,100, false);
            Tile = new TileGameObject(0, 0);
            Tile.SetTileID(0);

            //Tile = new TileGameObject(0, 0);
            //Tile.SetTileID(0);

            GL.Enable(EnableCap.DepthTest);

            base.OnLoad();
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
            TileGrid?.Dispose();
            ShaderFactory.Dispose();

            base.OnUnload();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
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

            for (int i = timeSpans.Count-1; i >= 0; i--)
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
            TileGrid.GPU_Use();
            Tile.GPU_Use();

            GL.Clear(ClearBufferMask.DepthBufferBit); //Clear depth buffer for ui to be on top

            GL.UseProgram(ShaderFactory.ShaderPrograms["TextShader.glsl"].ShaderProgramHandle);
            _font.RenderText($"FPS: {framesDuringLimit}", new Vector2(.5f, 12f), .25f, new Color4(1f, .8f, 1f, 1f));
            _font.RenderText($"IsBindlessSupported: {IsBindlessSupported}", new Vector2(.5f, 25f), .25f, new Color4(1f, .8f, 1f, 1f));
            _font.RenderText("this is a test", new Vector2(this.Size.X/2, this.Size.Y / 2), 1f, new Color4(.1f, .8f, .1f, .15f));

            this.Context.SwapBuffers(); //Take back buffer into forground buffer

            base.OnRenderFrame(args);
        }
    }
}
