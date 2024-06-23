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
using OpenGL.App.Management;

namespace OpenGL.App
{
    public class Game : GameWindow
    {
        private ShaderProgram _fontShaderProgram;
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
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, e.Width, e.Height);
            base.OnResize(e);
        }

        bool rainbowImage = false;

        protected override void OnLoad()
        {
            this.IsVisible = true;

            GL.ClearColor(Color4.DarkCyan); //Set up clear color

            int windowWidth = this.ClientSize.X;
            int windowHeight = this.ClientSize.Y;

            VertexPositionTexture[] vertices = new VertexPositionTexture[]
                {
                new VertexPositionTexture(new Vector2(.5f, .5f), new Vector2(1, 1)),
                new VertexPositionTexture(new Vector2(.5f, -.5f), new Vector2(1, 0)),
                new VertexPositionTexture(new Vector2(-.5f, -.5f), new Vector2(0, 0)),
                new VertexPositionTexture(new Vector2(-.5f, .5f), new Vector2(0, 1))
                };

            VertexColor[] verticesColor = new VertexColor[]
            {
                new VertexColor(new Color4(1f, 0f, 0f, 1f)),
                new VertexColor(new Color4(0f, 1f, 0f, 1f)),
                new VertexColor(new Color4(0f, 0f, 1f, 1f)),
                new VertexColor(new Color4(1f, 1f, 1f, 1f))
            };

            int[] indices = new int[]
            {
                0, 1, 2, 0, 2, 3
            };

            var vertexBuffer = new VertexBuffer(VertexPositionTexture.VertexInfo, vertices.Length, true);
            vertexBuffer.SetData(vertices, vertices.Length);

            var vertexColorBuffer = new VertexBuffer(VertexColor.VertexInfo, verticesColor.Length, false);
            vertexColorBuffer.SetData(verticesColor, verticesColor.Length);

            var _texture = ResourceManager.Instance.LoadTexture("C:\\tmp\\test.png");

            _gameObject = new PlaneWithImage(new Vector3(0.5f, 0.4f, 0.5f), GameObject.ProjectionTypeEnum.Orthographic, "Resources/Shaders/TextureWithColorAndTextureSlot.glsl", new VertexBuffer[] { vertexBuffer, vertexColorBuffer }, indices, _texture);



            this._fontShaderProgram = new ShaderProgram("Resources/Shaders/TextShader.glsl");

            _font = new FreeTypeFont();

            base.OnLoad();
        }

        protected override void OnUnload()
        {
            //Removing everything
            _gameObject.Dispose();

            base.OnUnload();
        }

        public class Event
        {

        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            _gameObject.Update(args);

            base.OnUpdateFrame(args);
        }

        List<long> timeSpans = new List<long>();
        TimeSpan limit = new TimeSpan(0, 0, 5);
        int framesDuringLimit = 0;

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

            GL.Clear(ClearBufferMask.ColorBufferBit); //Clear color buffer

            _gameObject.GPU_Use();

            GL.UseProgram(this._fontShaderProgram.ShaderProgramHandle);
            _font.RenderText(this._fontShaderProgram, this.Size.X, this.Size.Y, $"FPS: {framesDuringLimit}", .5f, 12f, .25f, new Vector3(1f, .8f, 1f));
            _font.RenderText(this._fontShaderProgram, this.Size.X, this.Size.Y, "this is a test", this.Size.X/2, this.Size.Y / 2, 1f, new Vector3(.1f, .8f, .1f));

            //GL.DrawArrays(PrimitiveType.Triangles, 0, 3); //Draw call to setup triangle on GPU //THIS IS ONLY FOR DIRECT COORDS

            this.Context.SwapBuffers(); //Take back buffer into forground buffer

            base.OnRenderFrame(args);
        }
    }
}
