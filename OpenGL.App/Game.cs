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
        private IndexBuffer indexBuffer;
        private ShaderProgram shaderProgram;
        private VertexArray vertexArray;

        private Texture2D _texture;

        public Game(int width = 1280, int height = 768) : base(
            GameWindowSettings.Default, 
            new NativeWindowSettings()
            {
                Title = "Hamburger and/or cheeseburger",
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

        protected override void OnLoad()
        {
            this.IsVisible = true;

            GL.ClearColor(Color4.DarkCyan); //Set up clear color

            int windowWidth = this.ClientSize.X;
            int windowHeight = this.ClientSize.Y;

            Random rand = new Random();           

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

            var colorVertexBuffer = new VertexBuffer(VertexColor.VertexInfo, verticesColor.Length, true);
            colorVertexBuffer.SetData(verticesColor, verticesColor.Length);

            this.indexBuffer = new IndexBuffer(indices.Length, true);
            this.indexBuffer.SetData(indices, indices.Length);

            this.vertexArray = new VertexArray(new VertexBuffer[] { this.vertexBuffer, colorVertexBuffer });

            //this.shaderProgram = new ShaderProgram("Resources/Shaders/TextureWithTextureSlot.glsl");
            this.shaderProgram = new ShaderProgram("Resources/Shaders/TextureWithColorAndTextureSlot.glsl");

            int[] viewport = new int[4];
            GL.GetInteger(GetPName.Viewport, viewport); //Retrieve info from gpu

            _texture = ResourceManager.Instance.LoadTexture("C:\\tmp\\test.png");
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
            this.shaderProgram?.Dispose();
            
            base.OnUnload();
        }

        public Color4 color4 = new Color4(1f, 0f, 0f, 1f);
        public Vector2 testPos = new Vector2(.5f, .5f);
        public bool flipColor = false;

        protected override void OnUpdateFrame(FrameEventArgs args)
        {


            //this.colorFactor += this.deltaColorFactor;

            //if (this.colorFactor >= 1f)
            //{
            //    this.colorFactor = 1f;
            //    this.deltaColorFactor *= -1f;
            //}

            //if (this.colorFactor <= 0f)
            //{
            //    this.colorFactor = 0f;
            //    this.deltaColorFactor *= -1f;
            //}


            //if (color4.R >= 1f)
            //    flipColor = false;
            //else if (color4.R <= 0f)
            //    flipColor = true;

            //if (flipColor)
            //{
            //    color4.R += (float)(0.3f * args.Time);
            //    testPos.X += (float)(0.3f * args.Time);
            //}
            //else
            //{
            //    color4.R -= (float)(0.3f * args.Time);
            //    testPos.X -= (float)(0.3f * args.Time);
            //}

            //Console.WriteLine($"color4: r {color4.R} g {color4.G} b {color4.B} a {color4.A}");

            ////this.shaderProgram.SetUniform("colorFactor", color4.R, color4.G);
            ////this.shaderProgram.SetUniform("colorFactor", color4.R, color4.G, color4.B, color4.A);
            //this.shaderProgram.SetAttribute("aPosition", testPos.X, testPos.Y);

            base.OnUpdateFrame(args);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit); //Clear color buffer

            GL.UseProgram(this.shaderProgram.ShaderProgramHandle); //Use shader program

            GL.BindVertexArray(this.vertexArray.VertexArrayHandle); //Use vertex array handle to grab the vec3's variable

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.indexBuffer.IndexBufferHandle); //Use indices array linked to vertex array above

            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0); //Tell it to draw with the indices array

            //GL.DrawArrays(PrimitiveType.Triangles, 0, 3); //Draw call to setup triangle on GPU //THIS IS ONLY FOR DIRECT COORDS

            this.Context.SwapBuffers(); //Take back buffer into forground buffer

            base.OnRenderFrame(args);
        }
    }
}
