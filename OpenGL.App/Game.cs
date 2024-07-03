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

namespace OpenGL.App
{
    public class Game : GameWindow
    {
        public static Game _Game;

        public Camera MainCamera;

        public ShaderFactory ShaderFactory { get; private set; }
        private FreeTypeFont _font;

        private PlaneWithImage _gameObject;
        private PlaneWithImage _gameObject2;

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
                //Console.WriteLine($"Mouse DeltaX: {obj.DeltaX} DeltaY: {obj.DeltaY}");
                MainCamera.ProcessMouseMovement(obj.DeltaX, obj.DeltaY);
            }
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, e.Width, e.Height);
            base.OnResize(e);

            projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), this.ClientSize.X / this.ClientSize.Y, .1f, 100f);
        }

        bool rainbowImage = false;

        FmodAudio.FmodSystem fmodSystem;
        FmodAudio.Channel fmodChannel;
        FmodAudio.Sound sound;

        protected override void OnLoad()
        {
            this.IsVisible = true;

            fmodSystem = FmodAudio.Fmod.CreateSystem();
            fmodSystem.Init(32, FmodAudio.InitFlags.Normal);

            sound = fmodSystem.CreateSound("Resources/Sounds/close_door_metal.ogg");
            //fmodSystem.PlaySound(sound);

            MainCamera = new Camera(new Vector3(0, 0, 3f), null);

            GL.ClearColor(Color4.DarkCyan); //Set up clear color

            ShaderFactory = new ShaderFactory();

            Resources.Shaders.VertexPositionTexture[] vertices = new Resources.Shaders.VertexPositionTexture[]
                {
                new Resources.Shaders.VertexPositionTexture(new Vector2(.5f, .5f), new Vector2(1, 1)),
                new Resources.Shaders.VertexPositionTexture(new Vector2(.5f, -.5f), new Vector2(1, 0)),
                new Resources.Shaders.VertexPositionTexture(new Vector2(-.5f, -.5f), new Vector2(0, 0)),
                new Resources.Shaders.VertexPositionTexture(new Vector2(-.5f, .5f), new Vector2(0, 1))
                };

            Resources.Shaders.VertexColor[] verticesColor = new Resources.Shaders.VertexColor[]
            {
                new Resources.Shaders.VertexColor(new Color4(1f, 0f, 0f, 1f)),
                new Resources.Shaders.VertexColor(new Color4(0f, 1f, 0f, 1f)),
                new Resources.Shaders.VertexColor(new Color4(0f, 0f, 1f, 1f)),
                new Resources.Shaders.VertexColor(new Color4(1f, 1f, 1f, 1f))
            };

            int[] indices = new int[]
            {
                0, 1, 2, 0, 2, 3
            };

            var vertexBuffer = new VertexBuffer(Resources.Shaders.VertexPositionTexture.VertexInfo, vertices.Length, "PositionAndTexture", true);
            vertexBuffer.SetData(vertices, vertices.Length);

            var vertexColorBuffer = new VertexBuffer(Resources.Shaders.VertexColor.VertexInfo, verticesColor.Length, "Color", false);
            vertexColorBuffer.SetData(verticesColor, verticesColor.Length);

            var _texture = TextureFactory.Instance.LoadTexture("C:\\tmp\\test.png");

            _gameObject = new PlaneWithImage(new Vector3(0.5f, 0.4f, 0.5f), Vector3.One, new Quaternion(MathHelper.DegreesToRadians(0), MathHelper.DegreesToRadians(45), MathHelper.DegreesToRadians(0)), GameObject.ProjectionTypeEnum.Orthographic, "TextureWithColorAndTextureSlot.glsl", new VertexBuffer[] { vertexBuffer, vertexColorBuffer }, indices, _texture);
            _gameObject2 = new PlaneWithImage(new Vector3(0.7f, -.75f, 0.5f), new Vector3(1, .4f, 1), Quaternion.Identity, GameObject.ProjectionTypeEnum.Orthographic, "TextureWithColorAndTextureSlot.glsl", new VertexBuffer[] { vertexBuffer, vertexColorBuffer }, indices, _texture);

            _font = new FreeTypeFont();

            Stopwatch.Start();

            projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), this.ClientSize.X / this.ClientSize.Y, .1f, 100f);

            GL.Enable(EnableCap.DepthTest);

            base.OnLoad();
        }

        protected override void OnUnload()
        {
            //Removing everything
            _gameObject.Dispose();
            _gameObject2.Dispose();
            ShaderFactory.Dispose();

            fmodSystem.Release();

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

            _gameObject.Update(args);
            _gameObject2.Update(args);

            base.OnUpdateFrame(args);
        }

        List<long> timeSpans = new List<long>();
        TimeSpan limit = new TimeSpan(0, 0, 5);
        int framesDuringLimit = 0;

        Stopwatch Stopwatch = new Stopwatch();

        Matrix4 projection;

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            fmodSystem.Update();

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

            Console.WriteLine($"Camera Pos: {MainCamera.Position} Camera Front: {MainCamera.Front} Big Pos: {_gameObject.Position}");

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit); //Clear color buffer

            ShaderFactory.ShaderPrograms[_gameObject.ShaderFactoryID].SetUniform("view", MainCamera.GetViewMatrix()); //TODO put view and projection in Camera.cs
            ShaderFactory.ShaderPrograms[_gameObject.ShaderFactoryID].SetUniform("projection", projection);
            ShaderFactory.ShaderPrograms[_gameObject.ShaderFactoryID].SetUniform("model", _gameObject.ModelView);
            _gameObject.GPU_Use();
            ShaderFactory.ShaderPrograms[_gameObject.ShaderFactoryID].SetUniform("model", _gameObject2.ModelView);
            _gameObject2.GPU_Use();

            GL.UseProgram(ShaderFactory.ShaderPrograms["TextShader.glsl"].ShaderProgramHandle);
            _font.RenderText($"FPS: {framesDuringLimit}", new Vector2(.5f, 12f), .25f, new Color4(1f, .8f, 1f, 1f));
            _font.RenderText("this is a test", new Vector2(this.Size.X/2, this.Size.Y / 2), 1f, new Color4(.1f, .8f, .1f, .15f));

            this.Context.SwapBuffers(); //Take back buffer into forground buffer

            base.OnRenderFrame(args);
        }
    }
}
