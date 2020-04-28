using ObjectTK.Textures;
using OpenTK.Graphics.OpenGL;

using McTexture = MinecraftDotNet.Core.Resources.Texture;
using GlTexture = ObjectTK.Textures.Texture2D;

namespace MinecraftDotNet.ClientSide.Graphics.OpenGl
{
    public class GlTextureLoader : IGlTextureLoader
    {
        public delegate void TextureConfigureHandler(GlTexture texture);

        private readonly TextureConfigureHandler _textureConfigureHandler;

        public GlTextureLoader(TextureConfigureHandler textureConfigureHandler)
        {
            _textureConfigureHandler = textureConfigureHandler;
        }
        
        public Texture2D LoadTexture(McTexture texture)
        {
            var glTexture = new GlTexture(SizedInternalFormat.Rgba8, texture.Bitmap.Width, texture.Bitmap.Height);
            
            _textureConfigureHandler(glTexture);
            // glTexture.SetParameter(TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Nearest);
            // glTexture.SetParameter(TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest);
            
            glTexture.LoadBitmap(texture.Bitmap);
            
            return glTexture;
        }

        public void Unload(Texture2D glTexture)
        {
            glTexture.Dispose();
        }
    }
}