using OpenGL.App.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenGL.App.Management
{
    public sealed class ResourceManager
    {
        private static ResourceManager instance = null;
        private static readonly object _loc = new();
        private IDictionary<string, Texture2D> _textureCache = new Dictionary<string, Texture2D>();

        public static ResourceManager Instance
        {
            get
            {
                lock (_loc)
                {
                    if (instance == null)
                    {
                        instance = new ResourceManager();
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

            texture = TextureFactory.Load(textureName);
            _textureCache.Add(textureName, texture);
            return texture;
        }
    }
}
