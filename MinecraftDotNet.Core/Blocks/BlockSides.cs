using System;
using System.Collections.Generic;
using MinecraftDotNet.Core.Resources;

namespace MinecraftDotNet.Core.Blocks
{
    public class BlockSides
    {
        public BlockSides(IReadOnlyList<Texture> textures)
        {
            if (textures.Count != 6) 
                throw new ArgumentException("Block must have only 6 sides.");
            
            Textures = textures;
        }

        public IReadOnlyList<Texture> Textures { get; }

        public Texture LeftTexture  => Textures[0];
        public Texture RightTexture => Textures[1];
        public Texture DownTexture  => Textures[2];
        public Texture UpTexture    => Textures[3];
        public Texture BackTexture  => Textures[4];
        public Texture FrontTexture => Textures[5];
    }
}