using System;
using System.Drawing;
using System.IO;
using MinecraftDotNet.Core.Items;
using ObjectTK.Textures;
using OpenTK.Graphics.OpenGL;

namespace MinecraftDotNet.Core.Blocks
{
    public static class HcBlocks
    {
        public static BlockInfo Air { get; }
        
        public static BlockInfo Dirt { get; }

        static HcBlocks()
        {
            var dirtBitmap = new Bitmap("/home/vlad/Документы/Проекты/Rider/minecraftdotnet/assets/textures/girt.png");
            var dirtTex = new Texture2D(SizedInternalFormat.Rgba8, dirtBitmap.Width, dirtBitmap.Height);
            dirtTex.LoadBitmap(dirtBitmap);
            Dirt = new BlockInfo(new ItemInfo("dirt", 64), new BlockSides(new[]{dirtTex,dirtTex,dirtTex,dirtTex,dirtTex,dirtTex}));
            
            var airBitmap = new Bitmap(16, 16);
            var airTex = new Texture2D(SizedInternalFormat.Rgba8, airBitmap.Width, airBitmap.Height);
            airTex.LoadBitmap(airBitmap);
            Air = new BlockInfo(new ItemInfo("air", 0), new BlockSides(new[]{airTex,airTex,airTex,airTex,airTex,airTex,}));
        }
    }
}