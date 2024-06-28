using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace OpenGL.App.Core.Texture
{
    public sealed class TextureFactory
    {
        private static TextureFactory instance = null;
        private static readonly object _loc = new();
        private IDictionary<string, Texture2D> _textureCache = new Dictionary<string, Texture2D>();

        public static TextureFactory Instance
        {
            get
            {
                lock (_loc)
                {
                    if (instance == null)
                    {
                        instance = new TextureFactory();
                    }
                }

                return instance;
            }
        }

        public Texture2D LoadTexture(string textureName)
        {
            _textureCache.TryGetValue(textureName, out Texture2D texture);
            if (texture != null)
            {
                return texture;
            }

            texture = Load(textureName);
            _textureCache.Add(textureName, texture);
            return texture;
        }

        private static Texture2D Load(string textureName)
        {
            int handle = GL.GenTexture();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, handle);
            using (var image = new Bitmap(textureName))
            {
                image.RotateFlip(RotateFlipType.RotateNoneFlipY);
                var data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest); //Setting draw options for scaling
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

                return new Texture2D(handle);
            }
        }
    }
}
