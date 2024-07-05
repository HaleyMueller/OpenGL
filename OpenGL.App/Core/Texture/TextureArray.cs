using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace OpenGL.App.Core.Texture
{
    public class TextureArray : Texture
    {
        public int TextureHandle { get; private set; }

        public TextureArray(string directory)
        {
            Bitmap[] bitmaps = LoadImages(directory);

            // Generate the texture array
            TextureHandle = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2DArray, TextureHandle);

            // Define the texture array properties
            int mipmapLevels = (int)Math.Floor(Math.Log(Math.Max(bitmaps[0].Width, bitmaps[0].Height), 2)) + 1;

            /*
             Parameters:
                target: Specifies the target texture, which should be TextureTarget3d.Texture2DArray for a 2D texture array.
                levels: Specifies the number of mipmap levels.
                internalformat: Specifies the sized internal format of the texture.
                width: Specifies the width of the texture array.
                height: Specifies the height of the texture array.
                depth: Specifies the number of layers in the texture array (the number of 2D images).
             */
            GL.TexStorage3D(TextureTarget3d.Texture2DArray, mipmapLevels, SizedInternalFormat.Rgba8, bitmaps[0].Width, bitmaps[0].Height, bitmaps.Length);

            // Load each image into the texture array
            for (int i = 0; i < bitmaps.Length; i++)
            {
                BitmapData data = bitmaps[i].LockBits(new Rectangle(0, 0, bitmaps[i].Width, bitmaps[i].Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                /*
                 Parameters:
                    target: Specifies the target texture, which should be TextureTarget.Texture2DArray for a 2D texture array.
                    level: Specifies the mipmap level.
                    xoffset: Specifies the x-offset of the subregion to update.
                    yoffset: Specifies the y-offset of the subregion to update.
                    zoffset: Specifies the z-offset (layer) of the subregion to update. (When working with a 2D texture array, this effectively means the index of the layer you are targeting.)
                    width: Specifies the width of the subregion to update.
                    height: Specifies the height of the subregion to update.
                    depth: Specifies the depth of the subregion to update (should be 1 for a 2D texture array).
                    format: Specifies the format of the pixel data.
                    type: Specifies the data type of the pixel data.
                    pixels: Specifies a pointer to the image data in memory.
                 */
                GL.TexSubImage3D(TextureTarget.Texture2DArray, 0, 0, 0, i, bitmaps[i].Width, bitmaps[i].Height, 1, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

                bitmaps[i].UnlockBits(data);
                bitmaps[i].Dispose();
            }

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2DArray);

            // Set texture parameters
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        }

        public static Bitmap[] LoadImages(string directory)
        {
            string[] files = Directory.GetFiles(directory, "*.png");
            Bitmap[] bitmaps = new Bitmap[files.Length];

            for (int i = 0; i < files.Length; i++)
            {
                bitmaps[i] = new Bitmap(files[i]);
            }

            return bitmaps;
        }

        ~TextureArray()
        {
            Dispose();
        }

        public override void Dispose()
        {
            if (_disposed) return;

            _disposed = true;
            GL.DeleteTexture(TextureHandle);

            GC.SuppressFinalize(this);
        }

        public override void GPU_Use(TextureData textureData)
        {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2DArray, TextureHandle);
        }
    }
}
