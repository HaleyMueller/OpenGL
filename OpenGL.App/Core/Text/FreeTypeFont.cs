using OpenTK.Mathematics;
using SharpFont;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Runtime.InteropServices;

namespace OpenGL.App
{
    public class FreeTypeFont
    {
        Dictionary<uint, Character> _characters = new Dictionary<uint, Character>();
        int _vao;
        int _vbo;

        public FreeTypeFont() 
        { 
            Library library = new Library();

            Face face = new Face(library, "arial.ttf");
            face.SetPixelSizes(0, 48);

            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1); // disable byte-alignment restriction

            //// set texture unit
            //GL.ActiveTexture(TextureUnit.Texture0);

            

            for (uint i = 0; i < 128; i++)
            {
                try
                {
                    face.LoadChar(i, LoadFlags.Render, LoadTarget.Normal);

                    int textureID = GL.GenTexture();
                    GL.BindTexture(TextureTarget.Texture2D, textureID);
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R8, face.Glyph.Bitmap.Width, face.Glyph.Bitmap.Rows, 0, PixelFormat.Red, PixelType.UnsignedByte, face.Glyph.Bitmap.Buffer);

                    //Set texture params
                    GL.TextureParameter(textureID, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                    GL.TextureParameter(textureID, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                    GL.TextureParameter(textureID, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                    GL.TextureParameter(textureID, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

                    //using (Bitmap bitmap = FTBitmapToBitmap(face.Glyph.Bitmap))
                    //{
                    //    // Save the bitmap as a PNG file
                    //    bitmap.Save($"chars/{(char)i}.png", System.Drawing.Imaging.ImageFormat.Png);
                    //}

                    //Save character data
                    _characters.Add(i, new Character()
                    {
                        TextureID = textureID,
                        Size = new Vector2(face.Glyph.Bitmap.Width, face.Glyph.Bitmap.Rows),
                        Bearing = new Vector2(face.Glyph.BitmapLeft, face.Glyph.BitmapTop),
                        Advance = face.Glyph.Advance.X.Value
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            //// bind default texture
            //GL.BindTexture(TextureTarget.Texture2D, 0);

            //// set default (4 byte) pixel alignment 
            //GL.PixelStore(PixelStoreParameter.UnpackAlignment, 4);

            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            

            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();
            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);

            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * 6 * 4, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

        }

        static Bitmap FTBitmapToBitmap(FTBitmap ftBitmap)
        {
            int width = ftBitmap.Width;
            int height = ftBitmap.Rows;
            Bitmap bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    byte value = Marshal.ReadByte(ftBitmap.Buffer, y * ftBitmap.Pitch + x);
                    Color color = Color.FromArgb(value, value, value);
                    bitmap.SetPixel(x, y, color);
                }
            }

            return bitmap;
        }

        public void RenderText(ShaderProgram shaderProgram, int screenX, int screenY, string text, float x, float y, float scale, Vector3 color)
        {
            Matrix4 projectionM = Matrix4.CreateOrthographicOffCenter(0.0f, screenX, screenY, 0.0f, -1.0f, 1.0f);

            //var projection = Matrix4.CreateOrthographic(0.0f, 1280.0f, 0.0f, 768.0f);

            //shaderProgram.Use();
            GL.Uniform3(shaderProgram.GetUniformList().FirstOrDefault(x => x.Name == "textColor").Location, color.X, color.Y, color.Z);
            GL.UniformMatrix4(shaderProgram.GetUniformList().FirstOrDefault(x => x.Name == "projection").Location, false, ref projectionM);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindVertexArray(_vao);

            foreach (var c in text)
            {
                if (_characters.ContainsKey(c) == false)
                    continue;

                Character ch = _characters[c];

                float xpos = x + ch.Bearing.X * scale;
                float ypos = y - (ch.Size.Y - ch.Bearing.Y) * scale;

                float w = ch.Size.X * scale;
                float h = ch.Size.Y * scale;

                // update VBO for each character
                float[,] vertices = new float[6, 4]{
                    { xpos,     ypos - h,   0.0f, 0.0f },
                    { xpos,     ypos,       0.0f, 1.0f },
                    { xpos + w, ypos,       1.0f, 1.0f },

                    { xpos,     ypos - h,   0.0f, 0.0f },
                    { xpos + w, ypos,       1.0f, 1.0f },
                    { xpos + w, ypos - h,   1.0f, 0.0f }
                };

                //float[,] vertices = new float[6,4]
                //{  
                //    { 0.0f, -1.0f,   0.0f, 0.0f},
                //    { 0.0f,  0.0f,   0.0f, 1.0f},
                //    { 1.0f,  0.0f,   1.0f, 1.0f},
                //    { 0.0f, -1.0f,   0.0f, 0.0f},
                //    { 1.0f,  0.0f,   1.0f, 1.0f},
                //    { 1.0f, -1.0f,   1.0f, 0.0f}
                //};

                GL.BindTexture(TextureTarget.Texture2D, ch.TextureID);
                //GL.GenerateTextureMipmap(ch.TextureID);
                GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
                GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, (sizeof(float) * 4 * 6), vertices);

                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

                x += (ch.Advance >> 6) * scale;
            }

            GL.BindVertexArray(0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public struct Character
        {
            public int TextureID { get; set; }
            public Vector2 Size { get; set; }
            public Vector2 Bearing { get; set; }
            public int Advance { get; set; }
        }

    }
}
