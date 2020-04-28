using GlTexture = ObjectTK.Textures.Texture2D;
using McTexture = MinecraftDotNet.Core.Resources.Texture;

namespace MinecraftDotNet.ClientSide.Graphics.OpenGl
{
    public interface IGlTextureLoader
    {
        GlTexture LoadTexture(McTexture texture);
        void Unload(GlTexture glTexture);
    }
}