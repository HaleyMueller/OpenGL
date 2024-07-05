using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace OpenGL.App.Core.Texture
{
    public class Texture2D : Texture
    {
        public int Handle { get; private set; }

        public Texture2D(int handle)
        {
            Handle = handle;
        }

        public override void GPU_Use(TextureData textureData)
        {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, Handle);
        }

        ~Texture2D()
        {
            Dispose();
        }

        public override void Dispose()
        {
            if (_disposed) return;

            _disposed = true;
            GL.DeleteTexture(Handle);

            GC.SuppressFinalize(this);
        }
    }
}
