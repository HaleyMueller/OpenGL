using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace OpenGL.App.Core.Texture
{
    public class BindlessTexture
    {
        public List<long> TextureHandles { get; set; } = new List<long>();

        public BindlessTexture(string directory)
        {
            var textures = LoadTextures(directory);
            TextureHandles = CreateBindlessTextures(textures);
        }

        private static List<long> CreateBindlessTextures(List<int> textures)
        {
            var handles = new List<long>();
            foreach (var texture in textures)
            {
                GL.BindTexture(TextureTarget.Texture2D, texture);
                long handle = GL.Arb.GetTextureHandle(texture);
                GL.Arb.MakeTextureHandleResident(handle);
                handles.Add(handle);
            }
            return handles;
        }

        private static List<int> LoadTextures(string directory)
        {
            var textures = new List<int>();
            var files = Directory.GetFiles(directory, "*.png");

            foreach (var file in files)
            {
                int texture = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, texture);

                using (var image = new Bitmap(file))
                {
                    var data = image.LockBits(
                        new Rectangle(0, 0, image.Width, image.Height),
                        ImageLockMode.ReadOnly,
                        System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                        data.Width, data.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

                    image.UnlockBits(data);
                }

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                GL.BindTexture(TextureTarget.Texture2D, 0);
                textures.Add(texture);
            }

            return textures;
        }
    }
}
