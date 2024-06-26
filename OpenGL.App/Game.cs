﻿using System;
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

namespace OpenGL.App
{
    public class Game : GameWindow
    {
        public static Game _Game;

        public ShaderFactory ShaderFactory { get; private set; }
        private FreeTypeFont _font;

        private PlaneWithImage _gameObject;

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

        protected override void OnLoad()
        {
            this.IsVisible = true;

            fmodSystem = FmodAudio.Fmod.CreateSystem();
            fmodSystem.Init(32, FmodAudio.InitFlags.Normal);

            sound = fmodSystem.CreateSound("Resources/Sounds/close_door_metal.ogg");
            //fmodSystem.PlaySound(sound);



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

            _gameObject = new PlaneWithImage(new Vector3(0.5f, 0.4f, 0.5f), GameObject.ProjectionTypeEnum.Orthographic, "TextureWithColorAndTextureSlot.glsl", new VertexBuffer[] { vertexBuffer, vertexColorBuffer }, indices, _texture);

            _font = new FreeTypeFont();

            Stopwatch.Start();

            base.OnLoad();
        }

        protected override void OnUnload()
        {
            //Removing everything
            _gameObject.Dispose();
            ShaderFactory.Dispose();

            fmodSystem.Release();

            base.OnUnload();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            _gameObject.Update(args);

            base.OnUpdateFrame(args);
        }

        List<long> timeSpans = new List<long>();
        TimeSpan limit = new TimeSpan(0, 0, 5);
        int framesDuringLimit = 0;

        Stopwatch Stopwatch = new Stopwatch();

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

            GL.Clear(ClearBufferMask.ColorBufferBit); //Clear color buffer

            Matrix4 model = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(-55f));
            //model = model * Matrix4.CreateRotationX((float)(Stopwatch.ElapsedMilliseconds * MathHelper.DegreesToRadians(1f) * 200));
            Matrix4 view = Matrix4.CreateTranslation(0f, 0f, -5f);
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), this.ClientSize.X / this.ClientSize.Y, .1f, 100f);
            ShaderFactory.ShaderPrograms[_gameObject.ShaderFactoryID].SetUniform("model", model);
            ShaderFactory.ShaderPrograms[_gameObject.ShaderFactoryID].SetUniform("view", view);
            ShaderFactory.ShaderPrograms[_gameObject.ShaderFactoryID].SetUniform("projection", projection);
            _gameObject.GPU_Use();

            GL.UseProgram(ShaderFactory.ShaderPrograms["TextShader.glsl"].ShaderProgramHandle);
            _font.RenderText($"FPS: {framesDuringLimit}", new Vector2(.5f, 12f), .25f, new Color4(1f, .8f, 1f, 1f));
            _font.RenderText("this is a test", new Vector2(this.Size.X/2, this.Size.Y / 2), 1f, new Color4(.1f, .8f, .1f, .15f));

            //GL.DrawArrays(PrimitiveType.Triangles, 0, 3); //Draw call to setup triangle on GPU //THIS IS ONLY FOR DIRECT COORDS

            this.Context.SwapBuffers(); //Take back buffer into forground buffer

            base.OnRenderFrame(args);
        }
    }
}
