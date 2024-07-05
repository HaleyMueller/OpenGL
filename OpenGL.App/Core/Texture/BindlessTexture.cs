using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;
using System.Reflection.Metadata;

namespace OpenGL.App.Core.Texture
{
    public class BindlessTexture : Texture
    {
        public int TextureGroupHandle;
        public List<BindlessTextureHandle> BindlessTextureHandles { get; set; } = new List<BindlessTextureHandle>();

        public string[] TextureFiles { get; set; }

        public class BindlessTextureHandle
        {
            public long BindlessHandle;
            public int TextureHandle;
            //public string FileName;
        }

        public BindlessTexture(string directory)
        {
            LoadTextures(directory);
            CreateBindlessTextures();
        }

        private void CreateBindlessTextures()
        {
            foreach (var texture in BindlessTextureHandles)
            {
                GL.BindTexture(TextureTarget.Texture2D, texture.TextureHandle);
                texture.BindlessHandle = GL.Arb.GetTextureHandle(texture.TextureHandle);
                GL.Arb.MakeTextureHandleResident(texture.BindlessHandle);
            }
        }

        private void LoadTextures(string directory)
        {
            var files = Directory.GetFiles(directory, "*.png");

            TextureFiles = new string[files.Length];
            int i = 0;
            foreach (var file in files)
            {
                var bindlessTextureHandle = new BindlessTextureHandle();

                TextureFiles[i] = file;

                //bindlessTextureHandle.FileName = file;
                bindlessTextureHandle.TextureHandle = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, bindlessTextureHandle.TextureHandle);

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

                BindlessTextureHandles.Add(bindlessTextureHandle);
                i++;
            }
        }

        ~BindlessTexture()
        {
            Dispose();
        }

        public override void Dispose()
        {
            //if (_disposed) return;

            //_disposed = true;
            //foreach (var textureHandle in this.BindlessTextureHandles)
            //{
            //    GL.Arb.MakeTextureHandleNonResident(textureHandle.BindlessHandle);
            //    GL.DeleteTexture(textureHandle.TextureHandle);
            //}

            //GC.SuppressFinalize(this);
        }

        public override void GPU_Use(TextureData textureData)
        {
            if (textureData.ShaderUniformLocation == null || textureData.SelectedTexture == null)
                throw new ArgumentOutOfRangeException("Parameters shaderUniformLocation and selectedTexture cannot be null");

            GL.Arb.UniformHandle(textureData.ShaderUniformLocation.Value, BindlessTextureHandles[textureData.SelectedTexture.Value].BindlessHandle);
        }
    }
}
