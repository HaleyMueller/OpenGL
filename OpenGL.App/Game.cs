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

namespace OpenGL.App
{
    public class Game : GameWindow
    {
        private VertexBuffer vertexBuffer; //Reference to Vertices that will be on the gpu
        private IndexBuffer indexBuffer;
        private int shaderProgramHandle;
        private VertexArray vertexArray; 

        private int vertexCount = 0;
        private int indexCount = 0;

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

            int boxCount = 100;

            VertexPositionColor[] vertices = new VertexPositionColor[boxCount * 4]; //4 vertices per box
            this.vertexCount = 0;
            for (int i = 0; i < boxCount; i++)
            {
                int w = rand.Next(32, 128);
                int h = rand.Next(32, 128);
                int x = rand.Next(0, windowWidth - w);
                int y = rand.Next(0, windowHeight - h);

                float r = (float)rand.NextDouble();
                float g = (float)rand.NextDouble();
                float b = (float)rand.NextDouble();

                vertices[this.vertexCount++] = new VertexPositionColor(new Vector2(x, y + h), new Color4(r, g, b, 1f));
                vertices[this.vertexCount++] = new VertexPositionColor(new Vector2(x + w, y + h), new Color4(r, g, b, 1f));
                vertices[this.vertexCount++] = new VertexPositionColor(new Vector2(x + w, y), new Color4(r, g, b, 1f));
                vertices[this.vertexCount++] = new VertexPositionColor(new Vector2(x, y), new Color4(r, g, b, 1f));
            }

            int[] indices = new int[boxCount * 6]; //6 = 2 triangles indices that make up the box
            this.indexCount = 0;
            this.vertexCount = 0;
            for (int i = 0; i < boxCount; i++)
            {
                indices[this.indexCount++] = 0 + this.vertexCount;
                indices[this.indexCount++] = 1 + this.vertexCount;
                indices[this.indexCount++] = 2 + this.vertexCount;
                indices[this.indexCount++] = 0 + this.vertexCount;
                indices[this.indexCount++] = 2 + this.vertexCount;
                indices[this.indexCount++] = 3 + this.vertexCount;

                vertexCount += 4;
            }

            this.vertexBuffer = new VertexBuffer(VertexPositionColor.VertexInfo, vertices.Length, true);
            this.vertexBuffer.SetData(vertices, vertices.Length);

            this.indexBuffer = new IndexBuffer(indices.Length, true);
            this.indexBuffer.SetData(indices, indices.Length);

            this.vertexArray = new VertexArray(this.vertexBuffer);

            string vertexShaderCode = //positions
                @"
                #version 330 core

                uniform vec2 ViewportSize; //Uniforms variables that can be be pushed to gpu differently

                layout (location = 0) in vec2 aPosition; //attribute variables start with A
                layout (location = 1) in vec4 aColor;

                out vec4 vColor; //Goes to pixelShader code

                void main()
                {
                    float nx = aPosition.x / ViewportSize.x * 2f - 1f;
                    float ny = aPosition.y / ViewportSize.y * 2f - 1f;
                    gl_Position = vec4(nx, ny, 0f, 1f);

                    vColor = aColor;
                }
                ";

            string pixelShaderCode = //fragment shader; color setting; every pixel passing out
                @"
                #version 330 core

                in vec4 vColor;     //From FragmentShader       

                out vec4 pixelColor;

                void main()
                {
                    pixelColor = vColor;
                }
                ";

            int vertexShaderHandle = GL.CreateShader(ShaderType.VertexShader); //Grab vertex shader handle from gpu
            GL.ShaderSource(vertexShaderHandle, vertexShaderCode); //Give it the shader code
            GL.CompileShader(vertexShaderHandle); //Compile

            string vertexShaderInfo = GL.GetShaderInfoLog(vertexShaderHandle);
            if (vertexShaderInfo != string.Empty)
            {
                Console.WriteLine(vertexShaderInfo);
            }

            int pixelShaderHandle = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(pixelShaderHandle, pixelShaderCode);
            GL.CompileShader(pixelShaderHandle);

            string pixelShaderInfo = GL.GetShaderInfoLog(pixelShaderHandle);
            if (pixelShaderInfo != string.Empty)
            {
                Console.WriteLine(pixelShaderInfo);
            }

            //Combine the shaders into a shader program
            this.shaderProgramHandle =  GL.CreateProgram();
            GL.AttachShader(shaderProgramHandle, vertexShaderHandle); //Attach shader to program
            GL.AttachShader(shaderProgramHandle, pixelShaderHandle); //Attach shader to same program

            GL.LinkProgram(shaderProgramHandle); //Link all shaders to program

            //Get rid of shaders in RAM after giving shaders to GPU to keep
            GL.DetachShader(this.shaderProgramHandle, vertexShaderHandle);
            GL.DetachShader(this.shaderProgramHandle, pixelShaderHandle);
            GL.DeleteShader(vertexShaderHandle);
            GL.DeleteShader(pixelShaderHandle);

            int[] viewport = new int[4];
            GL.GetInteger(GetPName.Viewport, viewport); //Retrieve info from gpu
            GL.UseProgram(this.shaderProgramHandle); //Tell open gl what program we going to send array to
            int viewportSizeUniformLocation = GL.GetUniformLocation(this.shaderProgramHandle, "ViewportSize"); //Get the location on the shader code of the ViewportSize variable
            GL.Uniform2(viewportSizeUniformLocation, (float)viewport[2], (float)viewport[3]); //Set the location variable on the shader code with the value from the array
            GL.UseProgram(0); //Clear


            base.OnLoad();
        }

        protected override void OnUnload()
        {
            //Removing everything
            this.vertexArray?.Dispose();
            this.indexBuffer?.Dispose();
            this.vertexBuffer?.Dispose();

            GL.UseProgram(0);
            GL.DeleteProgram(this.shaderProgramHandle);
            base.OnUnload();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit); //Clear color buffer

            GL.UseProgram(this.shaderProgramHandle); //Use shader program

            GL.BindVertexArray(this.vertexArray.VertexArrayHandle); //Use vertex array handle to grab the vec3's variable

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.indexBuffer.IndexBufferHandle); //Use indices array linked to vertex array above

            GL.DrawElements(PrimitiveType.Triangles, this.indexCount, DrawElementsType.UnsignedInt, 0); //Tell it to draw with the indices array

            //GL.DrawArrays(PrimitiveType.Triangles, 0, 3); //Draw call to setup triangle on GPU //THIS IS ONLY FOR DIRECT COORDS

            this.Context.SwapBuffers(); //Take back buffer into forground buffer

            base.OnRenderFrame(args);
        }
    }
}
