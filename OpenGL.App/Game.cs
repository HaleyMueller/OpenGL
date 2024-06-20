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
using OpenGL.App.Management;

namespace OpenGL.App
{
    public class Game : GameWindow
    {
        private VertexBuffer vertexBuffer; //Reference to Vertices that will be on the gpu
        private VertexBuffer vertexColorBuffer; //Reference to Vertices that will be on the gpu
        private IndexBuffer indexBuffer;
        private ShaderProgram shaderProgram;
        private VertexArray vertexArray;

        private Texture2D _texture;

        private ShaderProgram _fontShaderProgram;
        private FreeTypeFont _font;

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

        bool showImage = false;

        protected override void OnLoad()
        {
            this.IsVisible = true;

            GL.ClearColor(Color4.DarkCyan); //Set up clear color

            int windowWidth = this.ClientSize.X;
            int windowHeight = this.ClientSize.Y;

            if (showImage)
            {

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

                this.vertexBuffer = new VertexBuffer(VertexPositionTexture.VertexInfo, vertices.Length, true);
                this.vertexBuffer.SetData(vertices, vertices.Length);

                this.vertexColorBuffer = new VertexBuffer(VertexColor.VertexInfo, verticesColor.Length, false);
                this.vertexColorBuffer.SetData(verticesColor, verticesColor.Length);

                this.indexBuffer = new IndexBuffer(indices.Length, true);
                this.indexBuffer.SetData(indices, indices.Length);

                this.vertexArray = new VertexArray(new VertexBuffer[] { this.vertexBuffer, this.vertexColorBuffer });

                //this.shaderProgram = new ShaderProgram("Resources/Shaders/TextureWithTextureSlot.glsl");
                this.shaderProgram = new ShaderProgram("Resources/Shaders/TextureWithColorAndTextureSlot.glsl");

                

                //int[] viewport = new int[4];
                //GL.GetInteger(GetPName.Viewport, viewport); //Retrieve info from gpu

                _texture = ResourceManager.Instance.LoadTexture("C:\\tmp\\test.png");
            }


            this._fontShaderProgram = new ShaderProgram("Resources/Shaders/TextShader.glsl");

            _font = new FreeTypeFont();
            //_texture.Use();

            //this.shaderProgram.SetUniform("texCoord", _texture.Handle);


            //this.shaderProgram.SetUniform("ViewportSize", (float)viewport[2], (float)viewport[3]);
            //this.shaderProgram.SetUniform("ColorFactor", colorFactor);

            base.OnLoad();
        }

        protected override void OnUnload()
        {
            //Removing everything
            this.vertexArray?.Dispose();
            this.indexBuffer?.Dispose();
            this.vertexBuffer?.Dispose();
            this.vertexColorBuffer?.Dispose();
            this.shaderProgram?.Dispose();
            
            base.OnUnload();
        }

        public Color4 color4 = new Color4(1f, 0f, 0f, 1f);
        public float colorFactor = 1f;
        public Vector2 testPos = new Vector2(.5f, .5f);
        public bool flipColor = false;

        VertexColor[] verticesColor = new VertexColor[]
        {
            new VertexColor(new Color4(1f, 0f, 0f, 1f)),
            new VertexColor(new Color4(0f, 1f, 0f, 1f)),
            new VertexColor(new Color4(0f, 0f, 1f, 1f)),
            new VertexColor(new Color4(1f, 1f, 1f, 1f))
        };

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            if (showImage)
            {
                if (colorFactor >= 1f)
                {
                    Console.WriteLine("unflipping");
                    flipColor = false;
                }
                else if (colorFactor <= 0f)
                {
                    Console.WriteLine("flipping");
                    flipColor = true;
                }

                if (flipColor)
                {
                    colorFactor += (float)(0.4f * args.Time);
                }
                else
                {
                    colorFactor -= (float)(0.4f * args.Time);
                }

                Console.WriteLine($"ColorFactor:{colorFactor}");

                //this.shaderProgram.SetUniform("colorFactor", color4.R, color4.G);

                var copiedVertColor = new VertexColor[verticesColor.Length];
                Array.Copy(verticesColor, copiedVertColor, verticesColor.Length);

                for (int i = 0; i < copiedVertColor.Length; i++)
                {
                    copiedVertColor[i].Color.R *= colorFactor;
                    copiedVertColor[i].Color.G *= colorFactor;
                    copiedVertColor[i].Color.B *= colorFactor;
                }

                this.vertexColorBuffer.SetData(copiedVertColor, copiedVertColor.Length);
            }

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

            if (showImage)
            {
                GL.UseProgram(this.shaderProgram.ShaderProgramHandle); //Use shader program
                _texture.Use();
                GL.BindVertexArray(this.vertexArray.VertexArrayHandle); //Use vertex array handle to grab the vec3's variable

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.indexBuffer.IndexBufferHandle); //Use indices array linked to vertex array above

                GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0); //Tell it to draw with the indices array
            }

            GL.UseProgram(this._fontShaderProgram.ShaderProgramHandle);
            _font.RenderText(this._fontShaderProgram, this.Size.X, this.Size.Y, $"FPS: {framesDuringLimit}", .5f, 12f, .25f, new Vector3(1f, .8f, 1f));
            _font.RenderText(this._fontShaderProgram, this.Size.X, this.Size.Y, "this is a test", this.Size.X/2, this.Size.Y / 2, 1f, new Vector3(.1f, .8f, .1f));

            //GL.DrawArrays(PrimitiveType.Triangles, 0, 3); //Draw call to setup triangle on GPU //THIS IS ONLY FOR DIRECT COORDS

            this.Context.SwapBuffers(); //Take back buffer into forground buffer

            base.OnRenderFrame(args);
        }
    }
}
