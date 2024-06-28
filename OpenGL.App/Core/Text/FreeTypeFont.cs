using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Runtime.InteropServices;
using static FreeTypeSharp.FT;
using static FreeTypeSharp.FT_LOAD;
using static FreeTypeSharp.FT_Render_Mode_;
using FreeTypeSharp;
using OpenGL.App.Core.Shader;
using OpenGL.App.Resources.Shaders;

namespace OpenGL.App
{
    public class FreeTypeFont
    {
        Dictionary<uint, Character> _characters = new Dictionary<uint, Character>();
        VertexArray _vao;
        VertexBuffer _vbo;

        public unsafe FreeTypeFont() 
        {
            FT_LibraryRec_* lib;
            FT_FaceRec_* face;
            CheckForError(FT_Init_FreeType(&lib));

            CheckForError(FT_New_Face(lib, (byte*)Marshal.StringToHGlobalAnsi("Resources/Fonts/arial.ttf"), 0, &face));
            CheckForError(FT_Set_Char_Size(face, 0, 16 * 48, 300, 300));

            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1); // disable byte-alignment restriction

            for (uint i = 0; i < 128; i++)
            {
                try
                {
                    var glyphIndex = FT_Get_Char_Index(face, i);
                    CheckForError(FT_Load_Glyph(face, glyphIndex, FT_LOAD_DEFAULT));
                    CheckForError(FT_Render_Glyph(face->glyph, FT_RENDER_MODE_NORMAL));

                    int textureID = GL.GenTexture();
                    GL.BindTexture(TextureTarget.Texture2D, textureID);
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R8, ((int)face->glyph->bitmap.width), ((int)face->glyph->bitmap.rows), 0, PixelFormat.Red, PixelType.UnsignedByte, (nint)face->glyph->bitmap.buffer);

                    //Set texture params
                    GL.TextureParameter(textureID, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                    GL.TextureParameter(textureID, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                    GL.TextureParameter(textureID, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                    GL.TextureParameter(textureID, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

                    if ((face->glyph->bitmap.rows == 0 || face->glyph->bitmap.pitch == 0) == false) //Prevent empty texture from being saved
                    { 
                        using (Bitmap bitmap = BitmapFromGlyphBitmap(face->glyph->bitmap))
                        {
                            bitmap.Save($"chars/{i}.png", System.Drawing.Imaging.ImageFormat.Png);
                        }
                    }

                    //Save character data
                    _characters.Add(i, new Character()
                    {
                        TextureID = textureID,
                        Size = new Vector2(face->glyph->bitmap.width, face->glyph->bitmap.rows),
                        Bearing = new Vector2(face->glyph->bitmap_left, face->glyph->bitmap_top),
                        Advance = face->glyph->advance.x.ToInt32()
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            _vbo = new VertexBuffer(TextShader.VertexInfo, 6, false);
            _vao = new VertexArray(new VertexBuffer[] {_vbo}, ShaderFactory.ShaderPrograms["TextShader.glsl"].ShaderProgramHandle);
        }

        public void CheckForError(FT_Error fT_Error)
        {
            if (fT_Error != FT_Error.FT_Err_Ok)
            {
                Console.WriteLine($"FreeTypeFont error: {fT_Error}");
            }
        }

        private unsafe static Bitmap BitmapFromGlyphBitmap(FT_Bitmap_ ftBitmap)
        {
            int width = (int)ftBitmap.width;
            int height = (int)ftBitmap.rows;
            int pitch = ftBitmap.pitch;

            // Create a new bitmap with the glyph dimensions
            Bitmap bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            // Lock the bitmap's bits
            System.Drawing.Imaging.BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.WriteOnly, bitmap.PixelFormat);

            // Copy the glyph buffer to the bitmap
            byte* buffer = (byte*)ftBitmap.buffer;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    byte grayValue = ftBitmap.buffer[y * pitch + x];
                    Color color = Color.FromArgb(grayValue, grayValue, grayValue, grayValue); // Grayscale
                    IntPtr pixelAddress = bitmapData.Scan0 + (y * bitmapData.Stride) + (x * 4);
                    Marshal.WriteInt32(pixelAddress, color.ToArgb());
                }
            }

            // Unlock the bits
            bitmap.UnlockBits(bitmapData);

            return bitmap;
        }

        public void RenderText(string text, Vector2 screenCoordinates, float scale, Color4 color)
        {
            Matrix4 projectionM = Matrix4.CreateOrthographicOffCenter(0.0f, Game._Game.ClientRectangle.Size.X, Game._Game.ClientRectangle.Size.Y, 0.0f, -1.0f, 1.0f);

            GL.Uniform4(ShaderFactory.ShaderPrograms["TextShader.glsl"].GetUniformList().FirstOrDefault(x => x.Name == "textColor").Location, color.R, color.G, color.B, color.A);
            GL.UniformMatrix4(ShaderFactory.ShaderPrograms["TextShader.glsl"].GetUniformList().FirstOrDefault(x => x.Name == "projection").Location, false, ref projectionM);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindVertexArray(this._vao.VertexArrayHandle);

            foreach (var c in text)
            {
                if (_characters.ContainsKey(c) == false)
                    continue;

                Character ch = _characters[c];

                float xpos = screenCoordinates.X + ch.Bearing.X * scale;
                float ypos = screenCoordinates.Y - (ch.Size.Y - ch.Bearing.Y) * scale;

                float w = ch.Size.X * scale;
                float h = ch.Size.Y * scale;

                // update VBO for each character
                TextShader[] vertices = new TextShader[]
                {
                    new TextShader(new Vector2(xpos, ypos-h), new Vector2(0, 0)),
                    new TextShader(new Vector2(xpos, ypos), new Vector2(0, 1)),
                    new TextShader(new Vector2(xpos+w, ypos), new Vector2(1, 1)),

                    new TextShader(new Vector2(xpos, ypos-h), new Vector2(0, 0)),
                    new TextShader(new Vector2(xpos+w, ypos), new Vector2(1, 1)),
                    new TextShader(new Vector2(xpos+w, ypos-h), new Vector2(1, 0))
                };

                GL.BindTexture(TextureTarget.Texture2D, ch.TextureID);
                _vbo.SetData(vertices);
                GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

                screenCoordinates.X += (ch.Advance >> 6) * scale;
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
