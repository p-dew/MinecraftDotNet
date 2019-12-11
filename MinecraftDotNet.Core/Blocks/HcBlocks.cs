using System.Drawing;
using MinecraftDotNet.Core.Items;
using ObjectTK.Textures;
using OpenTK.Graphics.OpenGL;

namespace MinecraftDotNet.Core.Blocks
{
    public static class HcBlocks
    {
        public static BlockInfo Air { get; }
        
        public static BlockInfo Dirt { get; }
        
        public static BlockInfo Test0 { get; }

        private static BlockInfo LoadBlockInfo(string texPath, string id)
        {
            var bitmap = new Bitmap(texPath);
            var tex = new Texture2D(SizedInternalFormat.Rgba8, bitmap.Width, bitmap.Height);
            tex.SetParameter(TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Nearest);
            tex.SetParameter(TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest);
            tex.LoadBitmap(bitmap);
            return new BlockInfo(new ItemInfo(id, 64), new BlockSides(new[]{tex,tex,tex,tex,tex,tex}));
        }
        
        static HcBlocks()
        {
            Dirt = LoadBlockInfo("./assets/textures/dirt.png", "dirt");
            Test0 = LoadBlockInfo("./assets/textures/test0.png", "test0");
            
            var airBitmap = new Bitmap(16, 16);
            var airTex = new Texture2D(SizedInternalFormat.Rgba8, airBitmap.Width, airBitmap.Height);
            airTex.LoadBitmap(airBitmap);
            //TODO: remove
            airTex = null;
            Air = new BlockInfo(new ItemInfo("air", 0), new BlockSides(new[]{airTex,airTex,airTex,airTex,airTex,airTex,}));
        }
    }
}