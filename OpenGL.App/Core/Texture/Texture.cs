using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace OpenGL.App.Core.Texture
{
    public class Texture : IDisposable
    {
        internal bool _disposed;

        //public TextureData Data { get; set; }

        public class TextureData
        {
            public int? ShaderUniformLocation;
            public int? SelectedTexture;
            public Dictionary<string, float> SetUniformOnAdd = new Dictionary<string, float>();
        }

        ~Texture()
        {
            Dispose();
        }

        public virtual void GPU_Use(TextureData textureData)
        {

        }

        public virtual void Dispose()
        {

        }
    }
}
