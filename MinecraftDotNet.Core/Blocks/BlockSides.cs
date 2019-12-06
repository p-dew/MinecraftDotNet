using System;
using System.Collections.Generic;
using MinecraftDotNet.Core.Graphics;
using ObjectTK.Textures;
using Texture = ObjectTK.Textures.Texture;

namespace MinecraftDotNet.Core.Blocks
{
    public class BlockSides
    {
        public BlockSides(IReadOnlyList<Texture2D> textures)
        {
            if (textures.Count != 6) 
                throw new ArgumentException("Block must have only 6 sides.");
            
            Textures = textures;
        }

        public IReadOnlyList<Texture2D> Textures { get; }

        public Texture2D RightTexture => Textures[0];
        public Texture2D LeftTexture  => Textures[1];
        public Texture2D UpTexture    => Textures[2];
        public Texture2D DownTexture  => Textures[3];
        public Texture2D FrontTexture => Textures[4];
        public Texture2D BackTexture  => Textures[5];
    }
}