using System.Collections.Generic;
using MinecraftDotNet.ClientSide.Graphics.Core;
using MinecraftDotNet.Core.Resources;
using ObjectTK.Textures;
using OpenTK.Graphics.OpenGL;

namespace MinecraftDotNet.ClientSide.Graphics.OpenGl
{
    using GlTexture = ObjectTK.Textures.Texture2D;
    using McTexture = MinecraftDotNet.Core.Resources.Texture;

    public class GlGraphics : IGlGraphics
    {
        private readonly IDictionary<ResourceId, GlTexture> _loadedTextures;
        private readonly IGlTextureLoader _textureLoader;
        
        public GlGraphics(IGlTextureLoader textureLoader)
        {
            _textureLoader = textureLoader;
            _loadedTextures = new Dictionary<ResourceId, GlTexture>();
        }

        public void LoadTexture(McTexture texture)
        {
            if (_loadedTextures.ContainsKey(texture.Id))
                return;

            _loadedTextures[texture.Id] = _textureLoader.LoadTexture(texture);
        }

        public void UnloadTexture(TextureId textureId)
        {
            var glTexture = _loadedTextures[textureId.ResourceId];
            _textureLoader.Unload(glTexture);
            _loadedTextures.Remove(textureId.ResourceId);
        }
    }
}